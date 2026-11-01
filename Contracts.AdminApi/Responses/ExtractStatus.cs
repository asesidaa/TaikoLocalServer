namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// "Run extract" (Server Management › Game data, setup): whether the server can run tools/extract itself
/// (<see cref="ExtractFound"/> next to the exe, <see cref="GamePath"/> set and present), and the last run.
/// </summary>
public record ExtractStatus(
    bool ExtractFound,
    string GamePath,
    bool GamePathFound,
    bool Running,
    bool Mods,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int? ExitCode,
    IReadOnlyList<string> Output);
