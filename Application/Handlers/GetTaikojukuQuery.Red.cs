using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private partial ValueTask<CommonTaikojukuResponse> HandleRed(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Red Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var red = gameDataService.Red();
        return ValueTask.FromResult(Ac15TaikojukuService.BuildResponse(
            request.RequestedDans,
            red.TaikojukuFileOrder,
            red.MusicInfoFileOrder,
            red.RedMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Red.Limits));
    }
}
