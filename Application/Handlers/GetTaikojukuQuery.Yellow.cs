using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;

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
            yellow.TaikojukuFileOrder.Select(MapYellowTaikojuku).ToArray(),
            yellow.MusicInfoFileOrder.Select(MapYellowMusic).ToArray(),
            yellow.YellowMusicInfos.Keys.ToArray(),
            Ac15EraProfiles.Yellow.Limits));
    }

    private static Ac15TaikojukuEntry MapYellowTaikojuku(YellowTaikojukuEntry entry) => new()
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

    private static Ac15MusicInfoEntry MapYellowMusic(YellowMusicInfoEntry entry) => new()
    {
        MusicId = entry.MusicId,
        SongNo = entry.SongNo,
        FileOrder = entry.FileOrder
    };
}
