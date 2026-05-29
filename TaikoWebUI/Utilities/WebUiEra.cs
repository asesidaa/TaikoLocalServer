namespace TaikoWebUI.Utilities;

public static class WebUiEra
{
    public const string Default = "Nijiiro";
    public const string Green = "Green";
    public const string Blue = "Blue";
    public static readonly string[] Supported = [Default, Green, Blue];
    private static readonly string[] Known = [Default, Green, Blue];

    public static bool IsSupported(string? era)
    {
        return Known.Any(value => string.Equals(value, era, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsGreen(string? era)
    {
        return string.Equals(era, Green, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsAc15(string? era)
    {
        return IsGreen(era) || string.Equals(era, Blue, StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? era)
    {
        return Known.FirstOrDefault(value => string.Equals(value, era, StringComparison.OrdinalIgnoreCase)) ?? Default;
    }

    public static IReadOnlyList<string> NormalizeEnabled(IEnumerable<string>? eras)
    {
        var normalized = eras?
            .Select(Normalize)
            .Where(IsSupported)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        return normalized.Count > 0 ? normalized : Supported;
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
}
