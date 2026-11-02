namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// One game in ServerSettings.json's Eras (Server Management › Status › Games, setup's Older games list).
/// <see cref="Folder"/> is where its files are read from, relative to the server when it's inside it. Nijiiro's load
/// detail is per file (<see cref="FilesLoaded"/> of <see cref="FilesTotal"/>); an older game's is <see cref="Error"/>.
/// Name counts are 0 when that game's names didn't load (or it has no name reader).
/// </summary>
public sealed record GameStatus(
    string Era,
    bool Enabled,
    string Folder,
    bool FolderFound,
    DateTime? LoadedAt,
    string? Error,
    int Songs,
    int Costumes,
    int Titles,
    int Tones,
    int? FilesLoaded = null,
    int? FilesTotal = null,
    uint? ShopSeason = null,
    string? DonChallengeRound = null);
