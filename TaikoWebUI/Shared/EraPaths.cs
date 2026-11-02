using TaikoWebUI.Components.Ui;

namespace TaikoWebUI.Shared;

/// <summary>
/// Which game a player page is for, and its address. Nijiiro keeps today's paths (/Users/1/PlayData/HighScores);
/// an older game adds its name after the player (/Users/1/Red/PlayData/HighScores, /Users/1/Red/Profile).
/// </summary>
public static class EraPaths
{
    public const string Nijiiro = "Nijiiro";
    public const string DonChallenge = "DonChallenge", Shop = "Shop", Battle = "Battle", Training = "Training", Ghost = "Ghost";

    /// <summary>The Play Data pages each game has, in the sidebar's order.</summary>
    public static IReadOnlyList<string> PagesOf(string game) => game == Nijiiro
        ? [PlayDataPaths.Stats, PlayDataPaths.Favourites, PlayDataPaths.HighScores, PlayDataPaths.PlayHistory, PlayDataPaths.DaniDojo]
        : [PlayDataPaths.HighScores, PlayDataPaths.PlayHistory, PlayDataPaths.DaniDojo,
            .. HasDonChallenge(game) ? [DonChallenge] : Array.Empty<string>(), .. HasShop(game) ? [Shop] : Array.Empty<string>(),
            .. game == "Blue" ? [Battle] : Array.Empty<string>(), .. HasTraining(game) ? [Training] : Array.Empty<string>(),
            .. game == "Green" ? [Ghost] : Array.Empty<string>()];

    public static bool HasDonChallenge(string game) => game is "Red" or "White";

    public static bool HasShop(string game) => game is "Green" or "Blue" or "Yellow";

    public static bool HasTraining(string game) => game is "Blue" or "White" or "Yellow";

    /// <summary>Green's friend list (the cabinet's), its own sidebar item. No other game has web Friends.</summary>
    public static string Friends(long baid) => $"/Users/{baid}/Green/Friends";

    /// <summary>An older game's name as written in a URL, or null for anything else (Nijiiro included).</summary>
    public static string? Older(string? segment) =>
        GameInfo.All.FirstOrDefault(g => g.Name != Nijiiro && string.Equals(g.Name, segment, StringComparison.OrdinalIgnoreCase))?.Name;

    public static string Index(long baid, string game) => game == Nijiiro ? PlayDataPaths.Index(baid) : $"/Users/{baid}/{game}/PlayData";

    public static string Of(long baid, string game, string page) => $"{Index(baid, game)}/{page}";

    public static string Profile(long baid, string game) => game == Nijiiro ? $"/Users/{baid}/Profile" : $"/Users/{baid}/{game}/Profile";

    /// <summary>The game a path is in: /Users/1/Red/… is Red, any other path Nijiiro (or no game at all).</summary>
    public static string GameOf(string path)
    {
        var parts = path.Split('?', '#')[0].Trim('/').Split('/');
        return parts.Length >= 3 && parts[0].Equals("Users", StringComparison.OrdinalIgnoreCase) && Older(parts[2]) is { } game ? game : Nijiiro;
    }

    /// <summary>
    /// The same page in another game: Profile, the Play Data index, or the Play Data page. A page the other game doesn't
    /// have goes to its address anyway, where the "isn't in" page says so. Null when the path isn't a player game page.
    /// </summary>
    public static string? Switch(string path, long baid, string to)
    {
        var parts = path.Split('?', '#')[0].Trim('/').Split('/');
        if (parts.Length < 3 || !parts[0].Equals("Users", StringComparison.OrdinalIgnoreCase)) return null;
        var rest = parts.Skip(Older(parts[2]) is null ? 2 : 3).ToArray();
        return rest switch
        {
            ["Profile", ..] => Profile(baid, to),
            ["PlayData"] => Index(baid, to),
            ["PlayData", var page, ..] => Of(baid, to, page),
            _ => null,
        };
    }
}
