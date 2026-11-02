namespace TaikoWebUI.Shared;

/// <summary>
/// The Play Data pages' addresses: the index at /Users/{baid}/PlayData and each page under it. The old
/// /Users/{baid}/Stats, /Songs, … still work (Pages/PlayDataRedirect.razor) but every link uses these.
/// </summary>
public static class PlayDataPaths
{
    public const string Stats = "Stats", Songs = "Songs", Favourites = "Favourites", HighScores = "HighScores",
        PlayHistory = "PlayHistory", DaniDojo = "DaniDojo";

    public static string Index(long baid) => $"/Users/{baid}/PlayData";

    public static string Of(long baid, string page) => $"{Index(baid)}/{page}";

    /// <summary>A song's page, opened on a difficulty when given.</summary>
    public static string Song(long baid, long songId, int? difficulty = null) =>
        $"{Of(baid, Songs)}/{songId}" + (difficulty is { } d ? $"?diff={d}" : "");
}
