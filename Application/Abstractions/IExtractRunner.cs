using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>Runs tools/extract on the configured GamePath, one run at a time, then reloads the game data.</summary>
public interface IExtractRunner
{
    ExtractStatus Status { get; }

    /// <summary>Starts a run in the background. Returns null when it started, else why it didn't.</summary>
    string? Start(bool mods);
}
