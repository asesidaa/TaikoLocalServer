using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private const int MaxDanSlots = 25;
    private const int MaxRequestedSlotsPerRequest = 11;
    private const int MaxSongsPerPack = 10;
    private const uint MaxGreenCourseLevel = 4;

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
