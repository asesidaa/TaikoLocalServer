namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// Extract (Server Management › Games, Game data, setup): whether the admin may send games' folders from their browser
/// instead of the server reading them from disk (<see cref="RemoteGameDir"/>, ServerSettings.AllowRemoteGameDir), every
/// game that's on, and the current or last run.
/// </summary>
public record ExtractStatus(bool RemoteGameDir, IReadOnlyList<ExtractGame> Games, ExtractRun? Run);

/// <summary>
/// One game that's on: where the server reads it from (<see cref="GamePath"/>, from the <see cref="Setting"/> named
/// there, empty when unset), whether its extracted files are there (<see cref="Extracted"/>), and what the last
/// extract wrote (<see cref="Last"/>, its extract.json; null before an extract that writes one).
/// </summary>
public record ExtractGame(string Game, string Setting, string GamePath, bool GamePathFound, bool Extracted, ExtractManifest? Last);

/// <summary>
/// A run of one or more games, one after another. <see cref="Steps"/> are the current (or last) game's, as extract
/// reports them; <see cref="ExitCodes"/> has each finished game's.
/// </summary>
public record ExtractRun(
    IReadOnlyList<string> Games,
    string? Current,
    bool Running,
    bool Mods,
    DateTime StartedAt,
    DateTime? FinishedAt,
    IReadOnlyList<ExtractStep> Steps,
    IReadOnlyDictionary<string, int> ExitCodes,
    IReadOnlyList<string> Output,
    ExtractProgress? Progress = null,
    bool Stopped = false);

/// <summary>Where the current game's extract is: the step (as <see cref="ExtractStep.Label"/>) and its number, counted as
/// steps start (extract prints its steps' counts only at the end), its items done out of
/// <see cref="Total"/> (0 while it doesn't know them), the last item done, the seconds since the step started and since
/// anything last moved (a slow step on a small machine still moves; a stall doesn't). Seconds, so the browser's clock
/// doesn't matter.</summary>
public record ExtractProgress(string Step, int Number, int Done, int Total, string? Item, double StepSeconds, double IdleSeconds);

/// <summary>What extract wrote for a game (data/&lt;game&gt;/extract.json, data/extract.json for Nijiiro).</summary>
public record ExtractManifest(string Era, DateTime FinishedAt, string Source, IReadOnlyList<ExtractStep> Steps, IReadOnlyList<ExtractNote> Notes);

/// <summary>One step of extract's report: "server files: 1440 written, 0 already up to date, 0 missing, 0 failed".</summary>
public record ExtractStep(string Label, int Written, int UpToDate, int Missing, int Failed);

/// <summary>
/// Something to know about a game's files. Kind: "gap" (this copy lacks something the game has), "absent" (the game
/// never had it) or "included". Code: early-config (Detail: the config used), no-battle, no-title-plates,
/// no-costume-icons, names.
/// </summary>
public record ExtractNote(string Kind, string Code, string? Detail);

/// <summary>
/// The game files extract reads (<c>TaikoLocalServer extract --inputs</c>), relative to the game folder with <c>/</c> (an older
/// game's to its USRDIR). A path ending in <c>/</c> is a whole folder. In <see cref="Names"/> folders only the file
/// names matter, so empty files are sent. <see cref="Mods"/> is the same for Data_mods (Nijiiro).
/// </summary>
public record ExtractInputs(IReadOnlyList<string> Files, IReadOnlyList<string> Names, ExtractInputs? Mods);

/// <summary>
/// A folder on the server, for picking a game's install (Server Management › Games, setup). <see cref="Path"/> null:
/// the drives (or <c>/</c>). <see cref="Parent"/> null at a drive's top. <see cref="IsGame"/>: it looks like the game's
/// install, so it can be picked.
/// </summary>
public record FolderListing(string? Path, string? Parent, bool IsGame, IReadOnlyList<FolderEntry> Entries, string? Error = null);

/// <summary>A folder (or an older game's .zip, <see cref="IsFile"/>) in a <see cref="FolderListing"/>.</summary>
public record FolderEntry(string Name, string Path, bool IsGame, bool IsFile = false);
