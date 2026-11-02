using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private partial ValueTask<CommonTaikojukuResponse> HandleMurasaki(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Murasaki Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var murasaki = gameDataService.Murasaki();
        return ValueTask.FromResult(Ac15TaikojukuService.BuildResponse(
            request.RequestedDans,
            murasaki.TaikojukuFileOrder,
            murasaki.MusicInfoFileOrder,
            murasaki.MurasakiMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Murasaki.Limits));
    }
}
