using System.Globalization;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15PlayDatetime
{
    private static readonly string[] Formats =
    [
        Constants.DateTimeFormat,
        "yyyy-MM-dd HH:mm:ss"
    ];

    public static DateTime ParseOrNow(string playDatetime)
        => DateTime.TryParseExact(
            playDatetime,
            Formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : DateTime.Now;
}
