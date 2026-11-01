using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>The server's recent log events in memory, for the live log in Server Management › Logs.</summary>
public interface ILogBuffer
{
    /// <summary>The backlog, oldest first.</summary>
    IReadOnlyList<LogLine> Snapshot();

    /// <summary>New events as they're logged, until <paramref name="cancellationToken"/> ends the stream.</summary>
    IAsyncEnumerable<LogLine> Subscribe(CancellationToken cancellationToken);
}
