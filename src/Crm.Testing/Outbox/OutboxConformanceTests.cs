using Crm.Mediator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace Crm.Testing.Outbox;

/// <summary>
/// The outbox, relay and inbox contract every module meets (ADR-009). Written once; each module's test project derives
/// from it with its own <see cref="IOutboxHarness"/> and a fresh database, and runs it against real SQL Server.
/// </summary>
public abstract class OutboxConformanceTests : IAsyncLifetime
{
    private IOutboxHarness? harness;

    /// <summary>The clock the outbox and relays read; tests move it to make retries due.</summary>
    protected FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 12, 14, 9, 0, 0, TimeSpan.Zero));

    private IOutboxHarness Harness => harness ??= CreateHarness(Clock);

    /// <summary>The module's harness over a migrated test database, reading <paramref name="clock"/>.</summary>
    protected abstract IOutboxHarness CreateHarness(TimeProvider clock);

    public async ValueTask InitializeAsync() => await Harness.ResetAsync();

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Outbox_Message_IsWrittenInSameTransactionAsEntityChange()
    {
        var before = await Harness.CountChangedEntitiesAsync(Cancellation);

        var messageId = await Harness.SaveChangeWithEventAsync(commit: true, Cancellation);

        (await Harness.CountChangedEntitiesAsync(Cancellation)).ShouldBe(before + 1);
        var message = (await Harness.FindOutboxMessageAsync(messageId, Cancellation)).ShouldNotBeNull();
        message.ProcessedAt.ShouldBeNull();
        message.Attempts.ShouldBe(0);
    }

    [Fact]
    public async Task Outbox_RolledBackChange_LeavesNoMessage()
    {
        var before = await Harness.CountChangedEntitiesAsync(Cancellation);

        var messageId = await Harness.SaveChangeWithEventAsync(commit: false, Cancellation);

        (await Harness.CountChangedEntitiesAsync(Cancellation)).ShouldBe(before);
        (await Harness.FindOutboxMessageAsync(messageId, Cancellation)).ShouldBeNull();
    }

    [Fact]
    public async Task Relay_PublishesPendingMessages_ThroughMediator_AndMarksProcessed()
    {
        var first = await Harness.SaveChangeWithEventAsync(commit: true, Cancellation);
        var second = await Harness.SaveChangeWithEventAsync(commit: true, Cancellation);
        var publisher = new RecordingEventPublisher();

        var published = await Harness.RunRelayOnceAsync(publisher, Cancellation);

        published.ShouldBe(2);
        publisher.Published.Select(e => e.MessageId).ShouldBe([first, second], "in the order they were written");
        (await Harness.FindOutboxMessageAsync(first, Cancellation)).ShouldNotBeNull().ProcessedAt.ShouldBe(Clock.GetUtcNow());
        (await Harness.FindOutboxMessageAsync(second, Cancellation)).ShouldNotBeNull().ProcessedAt.ShouldNotBeNull();
        (await Harness.RunRelayOnceAsync(publisher, Cancellation)).ShouldBe(0, "processed messages are not published again");
    }

    [Fact]
    public async Task Relay_Retries_FailedHandler_WithBackoff_ThenDeadLetters()
    {
        var messageId = await Harness.SaveChangeWithEventAsync(commit: true, Cancellation);
        var failing = new RecordingEventPublisher { OnPublish = (_, _) => throw new InvalidOperationException("subscriber failed") };
        var delays = new List<TimeSpan>();

        OutboxMessageState message;
        do
        {
            (await Harness.RunRelayOnceAsync(failing, Cancellation)).ShouldBe(0);
            message = (await Harness.FindOutboxMessageAsync(messageId, Cancellation)).ShouldNotBeNull();
            message.ProcessedAt.ShouldBeNull();
            if (message.DeadLetteredAt is null)
            {
                var attemptsSoFar = failing.Attempts;
                (await Harness.RunRelayOnceAsync(failing, Cancellation)).ShouldBe(0);
                failing.Attempts.ShouldBe(attemptsSoFar, "a failed message is not retried before its backoff ends");

                delays.Add(message.NextAttemptAt - Clock.GetUtcNow());
                Clock.SetUtcNow(message.NextAttemptAt);
            }
        }
        while (message.DeadLetteredAt is null && failing.Attempts < 100);

        message.DeadLetteredAt.ShouldNotBeNull("a message that keeps failing is dead-lettered");
        message.Attempts.ShouldBe(failing.Attempts);
        delays.Count.ShouldBeGreaterThan(1, "it is retried before it is dead-lettered");
        delays.ShouldBe(delays.Order().ToList(), "backoff never shrinks");
        delays[^1].ShouldBeGreaterThan(delays[0], "backoff grows");

        Clock.Advance(TimeSpan.FromDays(30));
        (await Harness.RunRelayOnceAsync(failing, Cancellation)).ShouldBe(0);
        failing.Attempts.ShouldBe(message.Attempts, "a dead-lettered message is not retried");
    }

    [Fact]
    public async Task TwoRelays_ForSameModule_OnlyOneProcesses()
    {
        var first = await Harness.SaveChangeWithEventAsync(commit: true, Cancellation);
        var second = await Harness.SaveChangeWithEventAsync(commit: true, Cancellation);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var blocked = new RecordingEventPublisher
        {
            OnPublish = async (_, cancellationToken) =>
            {
                entered.TrySetResult();
                await release.Task.WaitAsync(cancellationToken);
            },
        };

        var running = Harness.RunRelayOnceAsync(blocked, Cancellation);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(30), Cancellation);
        var other = new RecordingEventPublisher();
        var otherPublished = await Harness.RunRelayOnceAsync(other, Cancellation);
        release.SetResult();

        otherPublished.ShouldBe(0);
        other.Attempts.ShouldBe(0, "a second relay for the same module waits for none and publishes nothing while the first holds the lock");
        (await running).ShouldBe(2);
        blocked.Published.Select(e => e.MessageId).ShouldBe([first, second]);
    }

    [Fact]
    public async Task Relay_ResumesAfterRestart_WithoutLosingMessages()
    {
        Guid[] written =
        [
            await Harness.SaveChangeWithEventAsync(commit: true, Cancellation),
            await Harness.SaveChangeWithEventAsync(commit: true, Cancellation),
            await Harness.SaveChangeWithEventAsync(commit: true, Cancellation),
        ];
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(Cancellation);
        var beforeStop = new RecordingEventPublisher
        {
            OnPublish = async (_, _) => await stopping.CancelAsync(), // the host stops during the first delivery
        };

        await Should.ThrowAsync<OperationCanceledException>(() => Harness.RunRelayOnceAsync(beforeStop, stopping.Token));
        var afterRestart = new RecordingEventPublisher();
        var published = await Harness.RunRelayOnceAsync(afterRestart, Cancellation);

        beforeStop.Published.Count.ShouldBe(1, "the relay stops between messages when the host stops");
        published.ShouldBe(2);
        beforeStop.Published.Concat(afterRestart.Published).Select(e => e.MessageId).ShouldBe(written, "each message exactly once, in order");
        foreach (var messageId in written)
        {
            (await Harness.FindOutboxMessageAsync(messageId, Cancellation)).ShouldNotBeNull().ProcessedAt.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task Handler_ReceivingSameMessageTwice_RunsOnce()
    {
        var envelope = Envelope();

        await Harness.DeliverToInboxSubscriberAsync(envelope, fail: false, Cancellation);
        await Harness.DeliverToInboxSubscriberAsync(envelope, fail: false, Cancellation);

        (await Harness.CountInboxSubscriberChangesAsync(envelope.MessageId, Cancellation)).ShouldBe(1);
        (await SchemasHoldingTableAsync("InboxMessages")).ShouldBe([Harness.Schema], "the inbox is in the subscriber's own schema");
    }

    [Fact]
    public async Task Handler_FailingBeforeItsChangeIsSaved_RunsAgainOnRedelivery()
    {
        var envelope = Envelope();

        await Should.ThrowAsync<InvalidOperationException>(() => Harness.DeliverToInboxSubscriberAsync(envelope, fail: true, Cancellation));
        await Harness.DeliverToInboxSubscriberAsync(envelope, fail: false, Cancellation);

        (await Harness.CountInboxSubscriberChangesAsync(envelope.MessageId, Cancellation))
            .ShouldBe(1, "the inbox record is saved with the change, so a failed delivery is not marked received");
    }

    private MessageEnvelope Envelope() =>
        new(Guid.NewGuid(), "Crm.Testing.Outbox.ConformanceEvent", Clock.GetUtcNow(), "{}", Guid.NewGuid(), null, null);

    private async Task<List<string>> SchemasHoldingTableAsync(string table)
    {
        await using var connection = new SqlConnection(Harness.ConnectionString);
        await connection.OpenAsync(Cancellation);
        await using var command = new SqlCommand(
            "SELECT s.name FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE t.name = @table", connection);
        command.Parameters.AddWithValue("@table", table);
        await using var reader = await command.ExecuteReaderAsync(Cancellation);
        var schemas = new List<string>();
        while (await reader.ReadAsync(Cancellation))
        {
            schemas.Add(reader.GetString(0));
        }

        return schemas;
    }
}
