using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private partial ValueTask<CommonTaikojukuResponse> HandleWhite(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading White Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var white = gameDataService.White();
        return ValueTask.FromResult(Ac15TaikojukuService.BuildResponse(
            request.RequestedDans,
            white.TaikojukuFileOrder,
            white.MusicInfoFileOrder,
            white.WhiteMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.White.Limits));
    }
}
