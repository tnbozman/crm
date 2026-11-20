using Shouldly;
using Xunit;

namespace Crm.Testing.Outbox;

/// <summary>
/// The outbox, relay and inbox contract every module meets (ADR-009). Written once; each module's test project derives
/// from it with its own <see cref="IOutboxHarness"/> and a fresh database, and runs it against real SQL Server.
/// </summary>
public abstract class OutboxConformanceTests
{
    protected abstract IOutboxHarness Harness { get; }

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
}
