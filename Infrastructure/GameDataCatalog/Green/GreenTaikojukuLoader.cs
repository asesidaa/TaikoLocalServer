using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTaikojukuLoader
{
    public Task<IReadOnlyList<GreenTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.MusicMedleyInfoXml, cancellationToken);
    }

    public static async Task<IReadOnlyList<GreenTaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(path, cancellationToken);
        return entries.Select(Map).ToArray();
    }

    private static GreenTaikojukuEntry Map(Ac15TaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Conditions = Map(entry.Conditions),
        ExcellentConditions = Map(entry.ExcellentConditions),
        Songs = entry.Songs.Select(song => new GreenTaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static GreenTaikojukuConditions Map(Ac15TaikojukuConditions conditions) => new()
    {
        SoulGauge = conditions.SoulGauge,
        GoodCount = conditions.GoodCount,
        OkCount = conditions.OkCount,
        BadCount = conditions.BadCount,
        ComboCount = conditions.ComboCount,
        TotalHitCount = conditions.TotalHitCount,
        Score = conditions.Score,
        DrumrollCount = conditions.DrumrollCount
    };
}
