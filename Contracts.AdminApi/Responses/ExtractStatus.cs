namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// "Run extract" (Server Management › Game data, setup): where the server reads the game from (<see cref="GamePath"/>,
/// set and present), whether the admin may send the game folder from their browser instead (<see cref="RemoteGameDir"/>, ServerSettings.AllowRemoteGameDir), and
/// the last run.
/// </summary>
public record ExtractStatus(
    string GamePath,
    bool GamePathFound,
    bool RemoteGameDir,
    bool Running,
    bool Mods,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int? ExitCode,
    IReadOnlyList<string> Output);

/// <summary>
/// The game files extract reads (<c>TaikoLocalServer extract --inputs</c>), relative to the game folder with <c>/</c>. A path ending in
/// <c>/</c> is a whole folder. In <see cref="Names"/> folders only the file names matter, so empty files are sent.
/// <see cref="Mods"/> is the same for Data_mods.
/// </summary>
public record ExtractInputs(IReadOnlyList<string> Files, IReadOnlyList<string> Names, ExtractInputs? Mods);
