using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowTaikojukuLoader
{
    public const string VerupFileName = "yellow_taikojuku_verup_data.json";

    public Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(
            YellowGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), VerupFileName),
            cancellationToken);
    }

    public static async Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var parsePath = await CreateParseableCopyIfMissingFinalEntryCloseAsync(path, cancellationToken);
        try
        {
            return await Ac15TaikojukuLoader.LoadFromFileAsync(parsePath, cancellationToken);
        }
        finally
        {
            DeleteTemporaryCopy(parsePath, path);
        }
    }

    public static async Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadFromFileAsync(
        string path,
        string verupPath,
        CancellationToken cancellationToken)
    {
        var parsePath = await CreateParseableCopyIfMissingFinalEntryCloseAsync(path, cancellationToken);
        try
        {
            return await Ac15TaikojukuLoader.LoadFromFileAsync(
                parsePath,
                verupPath,
                nameof(GameEra.Yellow),
                cancellationToken);
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
}
