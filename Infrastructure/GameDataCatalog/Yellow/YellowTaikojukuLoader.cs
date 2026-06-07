using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowTaikojukuLoader
{
    public const string VerupFileName = "yellow_taikojuku_verup_data.json";

    public Task<IReadOnlyList<YellowTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(
            YellowGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), VerupFileName),
            cancellationToken);
    }

    public static async Task<IReadOnlyList<YellowTaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var parsePath = await CreateParseableCopyIfMissingFinalEntryCloseAsync(path, cancellationToken);
        try
        {
            var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(parsePath, cancellationToken);
            return entries.Select(Map).ToArray();
        }
        finally
        {
            DeleteTemporaryCopy(parsePath, path);
        }
    }

    public static async Task<IReadOnlyList<YellowTaikojukuEntry>> LoadFromFileAsync(
        string path,
        string verupPath,
        CancellationToken cancellationToken)
    {
        var parsePath = await CreateParseableCopyIfMissingFinalEntryCloseAsync(path, cancellationToken);
        try
        {
            var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(
                parsePath,
                verupPath,
                nameof(GameEra.Yellow),
                cancellationToken);
            return entries.Select(Map).ToArray();
        }
        finally
        {
            DeleteTemporaryCopy(parsePath, path);
        }
    }

    private static async Task<string> CreateParseableCopyIfMissingFinalEntryCloseAsync(
        string path,
        CancellationToken cancellationToken)
    {
        const string entryOpen = "<MusicMedleyInfoData";
        const string entryClose = "</MusicMedleyInfoData>";
        const string rootClose = "</boost_serialization>";

        var text = await File.ReadAllTextAsync(path, cancellationToken);
        var rootCloseIndex = text.LastIndexOf(rootClose, StringComparison.Ordinal);
        if (rootCloseIndex < 0)
        {
            return path;
        }

        var lastOpenIndex = text.LastIndexOf(entryOpen, rootCloseIndex, StringComparison.Ordinal);
        var lastCloseIndex = text.LastIndexOf(entryClose, rootCloseIndex, StringComparison.Ordinal);
        if (lastOpenIndex < 0 || lastCloseIndex > lastOpenIndex)
        {
            return path;
        }

        var newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var repaired = text.Insert(rootCloseIndex, $"  {entryClose}{newline}");
        var tempPath = Path.Combine(
            Path.GetTempPath(),
            $"yellow-musicmedleyinfo-{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(tempPath, repaired, cancellationToken);
        return tempPath;
    }

    private static void DeleteTemporaryCopy(string parsePath, string originalPath)
    {
        if (string.Equals(parsePath, originalPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        File.Delete(parsePath);
    }

    private static YellowTaikojukuEntry Map(Ac15TaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Conditions = Map(entry.Conditions),
        ExcellentConditions = Map(entry.ExcellentConditions),
        Songs = entry.Songs.Select(song => new YellowTaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static YellowTaikojukuConditions Map(Ac15TaikojukuConditions conditions) => new()
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
