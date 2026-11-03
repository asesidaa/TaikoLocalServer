using System.Globalization;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTelopQueryHandler
{
    private partial ValueTask<CommonGetTelopResponse> HandleNijiiro(GetTelopQuery request, CancellationToken cancellationToken)
    {
        var telops = gameDataService.Nijiiro().GetTelopDataDictionary();
        var telopData = request.TelopId == NijiiroTelop.CabinetId
            ? NijiiroTelop.Live(telops.Values, DateTime.Now)
            : telops.GetValueOrDefault(request.TelopId);
        if (telopData is null)
        {
            logger.LogInformation("No live telop for telop {TelopId}", request.TelopId);
            return ValueTask.FromResult(new CommonGetTelopResponse { Result = 1 });
        }

        return ValueTask.FromResult(new CommonGetTelopResponse
        {
            Result = 1,
            VerupNo = request.TelopId == NijiiroTelop.CabinetId ? NijiiroTelop.Version(telopData) : telopData.VerupNo,
            StartDatetime = telopData.StartDatetime,
            EndDatetime = telopData.EndDatetime,
            Telop = telopData.Telop
        });
    }
}

/// <summary>
/// Nijiiro cabinets show one ticker message, telop 1 (a cabinet with nothing cached asks only for it). So telop 1 is
/// whichever of telop_data.json's messages is live now: it has text and its window includes now, the latest start
/// winning. Its version changes when another message goes live or the live one is edited, so cabinets fetch it again.
/// </summary>
public static class NijiiroTelop
{
    public const uint CabinetId = 1;

    public static TelopData? Live(IEnumerable<TelopData> telops, DateTime now) => telops
        .Where(t => t.Telop.Length > 0 && (Parse(t.StartDatetime) ?? DateTime.MinValue) <= now && now <= (Parse(t.EndDatetime) ?? DateTime.MaxValue))
        .OrderByDescending(t => Parse(t.StartDatetime) ?? DateTime.MinValue).ThenByDescending(t => t.TelopId)
        .FirstOrDefault();

    /// <summary>Telop 1's version: 0 with nothing live, else unique to the live message and its own version.</summary>
    public static uint Version(TelopData? live) => live is null ? 0 : live.TelopId * 10_000 + live.VerupNo;

    private static DateTime? Parse(string s) =>
        DateTime.TryParseExact(s, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
