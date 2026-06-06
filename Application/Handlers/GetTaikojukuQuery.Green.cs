using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;

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
            green.TaikojukuFileOrder.Select(MapGreenTaikojuku).ToArray(),
            green.MusicInfoFileOrder.Select(MapGreenMusic).ToArray(),
            green.GreenMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Green.Limits,
            taikojukuVerupOffset: 0));
    }

    private static Ac15TaikojukuEntry MapGreenTaikojuku(GreenTaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Songs = entry.Songs.Select(song => new Ac15TaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static Ac15MusicInfoEntry MapGreenMusic(GreenMusicInfoEntry entry) => new()
    {
        MusicId = entry.MusicId,
        SongNo = entry.SongNo,
        FileOrder = entry.FileOrder
    };
}
