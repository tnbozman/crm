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
}
