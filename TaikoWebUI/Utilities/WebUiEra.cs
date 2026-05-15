namespace TaikoWebUI.Utilities;

public static class WebUiEra
{
    public const string Default = "Nijiiro";
    public static readonly string[] Supported = ["Nijiiro", "Green"];

    public static bool IsSupported(string? era)
    {
        return Supported.Any(value => string.Equals(value, era, StringComparison.OrdinalIgnoreCase));
    }

    public static string Normalize(string? era)
    {
        return Supported.FirstOrDefault(value => string.Equals(value, era, StringComparison.OrdinalIgnoreCase)) ?? Default;
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
