using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crm.__Module__.Persistence.Messaging;

/// <summary>Runs the __Module__ outbox relay in the host until it stops. Generated; keep in step with the template.</summary>
internal sealed partial class __Module__OutboxRelayService(IServiceScopeFactory scopes, TimeProvider clock, ILogger<__Module__OutboxRelayService> logger)
    : BackgroundService
{
    /// <summary>How long the relay waits when the outbox had nothing to publish, or after an error.</summary>
    public static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var published = 0;
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                published = await scope.ServiceProvider.GetRequiredService<__Module__OutboxRelay>()
                    .RunOnceAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception error)
            {
                // The database may be unavailable for a while: log, wait and try again rather than stop relaying.
                LogRelayFailed(error);
            }

            if (published == 0)
            {
                try
                {
                    await Task.Delay(IdleDelay, clock, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox relay __Module__ failed; retrying")]
    private partial void LogRelayFailed(Exception error);
}
