using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private partial ValueTask<CommonTaikojukuResponse> HandleKimidori(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Kimidori Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var kimidori = gameDataService.Kimidori();
        return ValueTask.FromResult(Ac15TaikojukuService.BuildResponse(
            request.RequestedDans,
            kimidori.TaikojukuFileOrder,
            kimidori.MusicInfoFileOrder,
            kimidori.KimidoriMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Kimidori.Limits));
    }
}
