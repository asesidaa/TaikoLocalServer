using System.Globalization;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTelopQueryHandler
{
    private partial ValueTask<CommonGetTelopResponse> HandleNijiiro(GetTelopQuery request, CancellationToken cancellationToken)
    {
        var live = NijiiroTelop.Live(gameDataService.Nijiiro().GetTelopDataDictionary().Values, DateTime.Now);
        if (live is null || NijiiroTelop.Text(request.TelopId, live.Telop) is not { } text)
        {
            logger.LogInformation("No live telop for telop {TelopId}", request.TelopId);
            return ValueTask.FromResult(new CommonGetTelopResponse { Result = 1 });
        }

        return ValueTask.FromResult(new CommonGetTelopResponse
        {
            Result = 1,
            VerupNo = NijiiroTelop.Version(live),
            StartDatetime = live.StartDatetime,
            EndDatetime = live.EndDatetime,
            Telop = text
        });
    }
}

/// <summary>
/// The 39.06 cabinet files each ticker message into a slot by a leading "/key " tag, and shows only tagged text. The
/// keys are S, Cp, C, Mp and M (any other key crashes the game), and which slot a cabinet shows isn't known. So the
/// message live now (it has text and its window includes now, the latest start winning) is served once per key, as
/// telops 1-5, each tagged. The tag goes first whatever the text is, so text an admin starts with "/" can't crash it.
/// </summary>
public static class NijiiroTelop
{
    public static readonly string[] Keys = ["S", "Cp", "C", "Mp", "M"];

    /// <summary>The telop ids the cabinet is told about (one per key).</summary>
    public static IEnumerable<uint> Ids => Enumerable.Range(1, Keys.Length).Select(i => (uint)i);

    public static TelopData? Live(IEnumerable<TelopData> telops, DateTime now) => telops
        .Where(t => t.Telop.Trim().Length > 0 && (Parse(t.StartDatetime) ?? DateTime.MinValue) <= now && now <= (Parse(t.EndDatetime) ?? DateTime.MaxValue))
        .OrderByDescending(t => Parse(t.StartDatetime) ?? DateTime.MinValue).ThenByDescending(t => t.TelopId)
        .FirstOrDefault();

    /// <summary>Telop <paramref name="id"/>'s text: the message tagged with that id's key (one line: the cabinet reads to
    /// the end of the line), or null for an id that has no key.</summary>
    public static string? Text(uint id, string message) =>
        id is >= 1 && id <= Keys.Length ? $"/{Keys[id - 1]} {string.Join(' ', message.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))}" : null;

    /// <summary>The telops' version: 0 with nothing live, else unique to the live message and its own version.</summary>
    public static uint Version(TelopData? live) => live is null ? 0 : live.TelopId * 10_000 + live.VerupNo;

    private static DateTime? Parse(string s) =>
        DateTime.TryParseExact(s, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
