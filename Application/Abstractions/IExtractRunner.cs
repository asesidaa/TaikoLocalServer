using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>
/// Runs tools/extract, one run at a time, then reloads the game data: on the configured GamePath, or on game files an
/// admin sent from their browser (ServerSettings.AllowRemoteGameDir).
/// </summary>
public interface IExtractRunner
{
    ExtractStatus Status { get; }

    /// <summary>Starts a run in the background on GamePath, or on the uploaded files. Returns null when it started, else why it didn't.</summary>
    string? Start(bool mods, bool uploaded = false);

    /// <summary>The files to send (<c>extract --inputs</c>), or null with why not.</summary>
    Task<(ExtractInputs? Inputs, string? Error)> InputsAsync(CancellationToken cancellationToken);

    /// <summary>Starts a new upload, removing the last one. Returns why not, or null.</summary>
    string? ClearUpload();

    /// <summary>Stores one uploaded file under its path in the game folder. Returns why not (a path extract doesn't read…), or null.</summary>
    Task<string?> SaveUploadAsync(string path, Stream content, CancellationToken cancellationToken);
}
