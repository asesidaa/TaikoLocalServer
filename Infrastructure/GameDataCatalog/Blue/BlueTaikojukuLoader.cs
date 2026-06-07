using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueTaikojukuLoader
{
    public const string VerupFileName = "blue_taikojuku_verup_data.json";

    public Task<IReadOnlyList<BlueTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(
            BlueGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.Blue), VerupFileName),
            cancellationToken);
    }

    public static async Task<IReadOnlyList<BlueTaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(path, cancellationToken);
        return entries.Select(Map).ToArray();
    }

    public static async Task<IReadOnlyList<BlueTaikojukuEntry>> LoadFromFileAsync(
        string path,
        string verupPath,
        CancellationToken cancellationToken)
    {
        var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(
            path,
            verupPath,
            nameof(GameEra.Blue),
            cancellationToken);
        return entries.Select(Map).ToArray();
    }

    private static BlueTaikojukuEntry Map(Ac15TaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Conditions = Map(entry.Conditions),
        ExcellentConditions = Map(entry.ExcellentConditions),
        Songs = entry.Songs.Select(song => new BlueTaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static BlueTaikojukuConditions Map(Ac15TaikojukuConditions conditions) => new()
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
