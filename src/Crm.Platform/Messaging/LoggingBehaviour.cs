using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Crm.Platform.Messaging;

/// <summary>Pipeline step 2: structured log of each request's type, correlation ID and duration. Never the payload.</summary>
internal sealed partial class LoggingBehaviour(MessageContext context, ILogger<LoggingBehaviour> logger) : IRequestBehaviour
{
    public async Task<TResponse> Handle<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> proceed, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(proceed);
        var requestType = typeof(TRequest).FullName!;
        var started = Stopwatch.GetTimestamp();
        LogHandling(requestType, context.CorrelationId);

        var response = await proceed().ConfigureAwait(false);

        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        LogHandled(requestType, context.CorrelationId, elapsedMs);
        return response;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handling {RequestType} (correlation {CorrelationId})")]
    private partial void LogHandling(string requestType, Guid correlationId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestType} (correlation {CorrelationId}) in {ElapsedMs} ms")]
    private partial void LogHandled(string requestType, Guid correlationId, double elapsedMs);
}
