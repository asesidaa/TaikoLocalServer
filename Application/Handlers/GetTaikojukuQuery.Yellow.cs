using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private partial ValueTask<CommonTaikojukuResponse> HandleYellow(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Yellow Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var yellow = gameDataService.Yellow();
        return ValueTask.FromResult(Ac15TaikojukuService.BuildResponse(
            request.RequestedDans,
            yellow.TaikojukuFileOrder,
            yellow.MusicInfoFileOrder,
            yellow.YellowMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Yellow.Limits));
    }
}
