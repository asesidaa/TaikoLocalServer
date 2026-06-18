namespace TaikoWebUI.Utilities;

public static class WebUiEra
{
    public const string Default = "Nijiiro";
    public const string Green = "Green";
    public const string Blue = "Blue";
    public const string Yellow = "Yellow";
    public const string Red = "Red";
    public const string White = "White";
    public static readonly string[] Supported = [Default, Green, Blue, Yellow, Red, White];
    private static readonly string[] Known = [Default, Green, Blue, Yellow, Red, White];

    public static bool IsSupported(string? era)
    {
        return TryNormalize(era, out _);
    }

    public static bool IsGreen(string? era)
    {
        return string.Equals(era, Green, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsAc15(string? era)
    {
        return IsGreen(era)
            || string.Equals(era, Blue, StringComparison.OrdinalIgnoreCase)
            || string.Equals(era, Yellow, StringComparison.OrdinalIgnoreCase)
            || string.Equals(era, Red, StringComparison.OrdinalIgnoreCase)
            || string.Equals(era, White, StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsOlderAc15DonChallenge(string? era)
    {
        return string.Equals(era, Red, StringComparison.OrdinalIgnoreCase)
            || string.Equals(era, White, StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? era)
    {
        return TryNormalize(era, out var normalized) ? normalized : Default;
    }

    public static string NormalizeOrDefault(string? era, string? defaultEra)
    {
        if (TryNormalize(era, out var normalized))
        {
            return normalized;
        }

        return TryNormalize(defaultEra, out var normalizedDefault)
            ? normalizedDefault
            : Default;
    }

    public static IReadOnlyList<string> NormalizeEnabled(IEnumerable<string>? eras)
    {
        if (eras is null)
        {
            return Supported;
        }

        var normalized = eras
            .Select(era => TryNormalize(era, out var value) ? value : null)
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalized;
    }

    public static bool TryNormalize(string? era, out string normalized)
    {
        normalized = Known.FirstOrDefault(value => string.Equals(value, era, StringComparison.OrdinalIgnoreCase))
                     ?? string.Empty;
        return normalized.Length > 0;
    }

    public static string UserRoute(uint baid, string? era, string page)
    {
        return $"Users/{baid}/{Normalize(era)}/{page.TrimStart('/')}";
    }

    public static string UserRoute(int baid, string? era, string page)
    {
        return UserRoute((uint)baid, era, page);
    }

    public static string Api(string? era, string path)
    {
        return $"api/{Normalize(era)}/{path.TrimStart('/')}";
    }

    public static int? GetFavoriteSongLimit(
        string? era,
        IReadOnlyDictionary<string, int>? favoriteSongLimits)
    {
        if (favoriteSongLimits is null)
        {
            return null;
        }

        var normalized = Normalize(era);
        if (favoriteSongLimits.TryGetValue(normalized, out var limit))
        {
            return PositiveLimitOrNull(limit);
        }

        foreach (var pair in favoriteSongLimits)
        {
            if (string.Equals(pair.Key, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return PositiveLimitOrNull(pair.Value);
            }
        }

        return null;
    }

    private static int? PositiveLimitOrNull(int limit)
        => limit > 0 ? limit : null;
}
