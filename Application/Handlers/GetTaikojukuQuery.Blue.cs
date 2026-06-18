using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private const int MaxBlueDanSlots = 25;
    private const int MaxBlueRequestedSlotsPerRequest = 11;
    private const int MaxBlueSongsPerPack = 10;
    private const uint MaxBlueCourseLevel = 4;

    private partial ValueTask<CommonTaikojukuResponse> HandleBlue(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Blue Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var blue = gameDataService.Blue();
        return ValueTask.FromResult(Ac15TaikojukuService.BuildResponse(
            request.RequestedDans,
            blue.TaikojukuFileOrder,
            blue.MusicInfoFileOrder,
            blue.BlueMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Blue.Limits));
    }
}
