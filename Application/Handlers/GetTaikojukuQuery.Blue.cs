using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;

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
            blue.TaikojukuFileOrder.Select(MapBlueTaikojuku).ToArray(),
            blue.MusicInfoFileOrder.Select(MapBlueMusic).ToArray(),
            blue.BlueMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Blue.Limits));
    }

    private static Ac15TaikojukuEntry MapBlueTaikojuku(BlueTaikojukuEntry entry) => new()
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

    private static Ac15MusicInfoEntry MapBlueMusic(BlueMusicInfoEntry entry) => new()
    {
        MusicId = entry.MusicId,
        SongNo = entry.SongNo,
        FileOrder = entry.FileOrder
    };
}
