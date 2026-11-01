namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// One data file as the server last loaded it (Server Management › Game data). <see cref="Editable"/> files are the
/// server-owned JSON in wwwroot/data; the rest are the game's datatables. A file that failed keeps its previous
/// contents in use and reports why in <see cref="Error"/>; <see cref="Missing"/> means it isn't there at all.
/// </summary>
public record GameDataFileStatus(string Name, bool Editable, bool Missing, string? Error, DateTime? Modified);
