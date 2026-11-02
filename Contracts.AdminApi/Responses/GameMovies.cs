namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// Server Management › Cabinets › Movies: the numbered movies in the game's movie folder (extract's movies.json; null
/// when extract hasn't run), and the admins' names for movies, which are server-wide.
/// </summary>
public record GameMovies(List<uint>? InGame, Dictionary<uint, string> Names);
