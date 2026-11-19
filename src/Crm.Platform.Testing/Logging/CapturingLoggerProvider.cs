using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Crm.Platform.Testing.Logging;

/// <summary>One captured log entry: level, category, and the formatted message followed by its structured values.</summary>
public sealed record CapturedLog(LogLevel Level, string Category, string Line);

/// <summary>Keeps every log entry, with its structured values, for assertions.</summary>
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    public ConcurrentQueue<CapturedLog> Entries { get; } = new();

    public IEnumerable<string> Lines => Entries.Select(e => e.Line);

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries, categoryName);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(ConcurrentQueue<CapturedLog> entries, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            var values = state is IEnumerable<KeyValuePair<string, object?>> pairs ? string.Join(" ", pairs.Select(p => $"{p.Key}={p.Value}")) : string.Empty;
            entries.Enqueue(new CapturedLog(logLevel, category, $"{formatter(state, exception)} | {values}"));
        }
    }
}
