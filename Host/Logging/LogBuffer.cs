using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Serilog.Core;
using Serilog.Events;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Logging;

/// <summary>
/// A Serilog sink keeping the last <see cref="Capacity"/> events for the live log, and fanning new ones out to
/// open log streams. Access codes are masked here, before anything leaves the server; the HeadClerk CSV lines stay out.
/// </summary>
public sealed partial class LogBuffer : ILogEventSink, ILogBuffer
{
    private const int Capacity = 2000;

    private readonly Queue<LogLine> lines = new();
    private readonly List<Channel<LogLine>> subscribers = [];
    private readonly Lock gate = new();
    private long nextId;

    [GeneratedRegex(@"\b\d{16}(\d{4})\b")]
    private static partial Regex AccessCode();

    private static string Mask(string text) => AccessCode().Replace(text, "••••$1");

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        if (message.StartsWith("CSV WRITE:", StringComparison.Ordinal)) return;
        var level = logEvent.Level switch
        {
            LogEventLevel.Verbose => "VRB", LogEventLevel.Debug => "DBG", LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN", LogEventLevel.Error => "ERR", _ => "FTL",
        };
        lock (gate)
        {
            var line = new LogLine(++nextId, logEvent.Timestamp.UtcDateTime, level, Mask(message),
                logEvent.Exception is { } e ? Mask(e.ToString()) : null);
            lines.Enqueue(line);
            if (lines.Count > Capacity) lines.Dequeue();
            foreach (var s in subscribers) s.Writer.TryWrite(line);
        }
    }

    public IReadOnlyList<LogLine> Snapshot()
    {
        lock (gate) return lines.ToList();
    }

    public async IAsyncEnumerable<LogLine> Subscribe([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // A slow reader loses its oldest pending lines rather than holding the server's memory.
        var channel = Channel.CreateBounded<LogLine>(new BoundedChannelOptions(500) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (gate) subscribers.Add(channel);
        try
        {
            await foreach (var line in channel.Reader.ReadAllAsync(cancellationToken))
                yield return line;
        }
        finally
        {
            lock (gate) subscribers.Remove(channel);
        }
    }
}
