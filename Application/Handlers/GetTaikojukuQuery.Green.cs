using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private partial ValueTask<CommonTaikojukuResponse> HandleGreen(GetTaikojukuQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Green Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var green = gameDataService.Green();
        return ValueTask.FromResult(Ac15TaikojukuService.BuildResponse(
            request.RequestedDans,
            green.TaikojukuFileOrder,
            green.MusicInfoFileOrder,
            green.GreenMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Green.Limits));
    }
}
