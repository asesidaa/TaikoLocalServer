using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>
/// Runs the extractor (Infrastructure.Extract) for one or more games, one after another, then reloads each game's data. Each game is read from
/// its install on the server (Nijiiro: Eras.Nijiiro.GameDataPath; the older games: Eras.&lt;Game&gt;.GamePath), or
/// from the files an admin sent from their browser (ServerSettings.AllowRemoteGameDir). One run at a time.
/// </summary>
public interface IExtractRunner
{
    ExtractStatus Status { get; }

    /// <summary>Starts a run of <paramref name="games"/> in the background. Returns null when it started, else why it didn't.</summary>
    string? Start(IReadOnlyList<GameEra> games, bool mods, IReadOnlySet<GameEra> uploaded);

    /// <summary>The files to send for a game (<c>extract --inputs</c>), or null with why not.</summary>
    Task<(ExtractInputs? Inputs, string? Error)> InputsAsync(GameEra game, CancellationToken cancellationToken);

    /// <summary>Starts a new upload of a game's folder, removing that game's last one. Returns why not, or null.</summary>
    string? ClearUpload(GameEra game);

    /// <summary>Stores one uploaded file under its path in the game's folder. Returns why not (a path extract doesn't read…), or null.</summary>
    Task<string?> SaveUploadAsync(GameEra game, string path, Stream content, CancellationToken cancellationToken);

    /// <summary>The folders in <paramref name="path"/> on the server (null: the drives), each marked when it looks like <paramref name="game"/>'s install.</summary>
    FolderListing Browse(GameEra game, string? path);

    /// <summary>Reads <paramref name="game"/> from <paramref name="path"/> from now on, and saves it in ServerSettings.json. Returns why not, or null.</summary>
    Task<string?> SetGamePathAsync(GameEra game, string path, CancellationToken cancellationToken);
}
