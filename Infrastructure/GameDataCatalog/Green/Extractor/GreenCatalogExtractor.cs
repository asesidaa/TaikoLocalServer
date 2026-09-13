using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;
using System.Text.RegularExpressions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public static partial class GreenCatalogExtractor
{
    public const string CostumeFileName = "green_costume_data.json";
    public const string TitleFileName = "green_title_data.json";
    public const string NeiroFileName = "green_neiro_data.json";

    public static async Task ExtractAsync(
        GreenExtractorOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cosNamePack = Path.Combine(options.GameDataPath, "nutdata", "cos_name", "nutdatapack.ndp");
        var titleNamePack = Path.Combine(options.GameDataPath, "nutdata", "title_name", "nutdatapack.ndp");
        var toneNamePack = Path.Combine(options.GameDataPath, "nutdata", "tone_name", "nutdatapack.ndp");
        var rewardTitleFiltering = Path.Combine(options.GameDataPath, "config", "S11100-1", "rewardtitlefiltering.xml");

        ValidateInputs(options.GameDataPath, [cosNamePack, titleNamePack, toneNamePack]);

        var cosEntries = ReadNamedPacks(
            options.GameDataPath,
            "cos_name",
            "costume_name",
            "costume_head_name",
            "costume_body_name",
            "reward_head_name",
            "reward_body_name");
        var titleEntries = ReadNamedPacks(options.GameDataPath, "title_name");
        // Green game data keeps cumulative title ID ranges in filename-only .nut/.ndp
        // sources (for example title_name_00649_00724.nut).  The legacy Green
        // extractor only parsed files named exactly nutdatapack.ndp, which limited
        // the generated catalog to the IDs present in the root pack (0..298 on
        // current Green data).  Scan all title_name catalog filenames as an
        // additional ID source, just like the older AC15 extractors do.
        var titleIds = ReadNamedCatalogIds(options.GameDataPath, "title_name");
        var toneEntries = ReadNamedPacks(options.GameDataPath, "tone_name");
        var rewardTitleIds = File.Exists(rewardTitleFiltering)
            ? await BoostXmlReader.ReadRewardTitleIdsAsync(rewardTitleFiltering, cancellationToken)
            : [];

        var overrides = await OverridesLoader.LoadAsync(options.OverridesPath, cancellationToken);
        _ = await new EbootStringResolver().ResolveAsync(options.EbootPath, cancellationToken);
        _ = await new WikiScraper().ResolveAsync(options.UseWiki, cancellationToken);

        var don3d = Don3dDirScanner.Scan(options.GameDataPath);
        var costumes = CostumeMerger.Merge(cosEntries, don3d, overrides);
        var titles = TitleMerger.Merge(titleEntries, titleIds, rewardTitleIds, overrides);
        var neiros = NeiroMerger.Merge(toneEntries, overrides);

        await CatalogWriter.WriteAsync(options.OutputDirectory, CostumeFileName, costumes, cancellationToken);
        await CatalogWriter.WriteAsync(options.OutputDirectory, TitleFileName, titles, cancellationToken);
        await CatalogWriter.WriteAsync(options.OutputDirectory, NeiroFileName, neiros, cancellationToken);
    }

    private static void ValidateInputs(string gameDataPath, IEnumerable<string> requiredFiles)
    {
        if (!Directory.Exists(gameDataPath))
        {
            throw new DirectoryNotFoundException($"Green game-data root does not exist: {gameDataPath}");
        }

        foreach (var requiredFile in requiredFiles)
        {
            if (!File.Exists(requiredFile))
            {
                throw new FileNotFoundException($"Required Green catalog source file does not exist: {requiredFile}", requiredFile);
            }
        }
    }


    private static IReadOnlyList<uint> ReadNamedCatalogIds(string gameDataPath, string catalogDirectoryName)
    {
        var nutdataRoot = Path.Combine(gameDataPath, "nutdata");
        if (!Directory.Exists(nutdataRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(nutdataRoot, "*", SearchOption.AllDirectories)
            .Where(path => string.Equals(
                Path.GetFileName(Path.GetDirectoryName(path)),
                catalogDirectoryName,
                StringComparison.OrdinalIgnoreCase))
            .Where(IsNameCatalogFile)
            .SelectMany(path => ParseCatalogIds(Path.GetFileNameWithoutExtension(path)))
            .Distinct()
            .Order()
            .ToArray();
    }

    private static bool IsNameCatalogFile(string path)
    {
        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".nut", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".ndp", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<uint> ParseCatalogIds(string fileName)
    {
        var matches = CatalogNumberRegex()
            .Matches(fileName)
            .Select(match => uint.Parse(match.Value))
            .ToArray();

        if (matches.Length == 0)
        {
            return [];
        }

        if (matches.Length >= 2)
        {
            var start = matches[^2];
            var end = matches[^1];
            if (start <= end && end - start <= 10000)
            {
                return Enumerable.Range((int)start, checked((int)(end - start + 1)))
                    .Select(id => (uint)id);
            }
        }

        return [matches[^1]];
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex CatalogNumberRegex();

    private static IReadOnlyList<NdpEntry> ReadNamedPacks(string gameDataPath, params string[] catalogDirectoryNames)
    {
        var nutdataRoot = Path.Combine(gameDataPath, "nutdata");
        var names = catalogDirectoryNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Directory.EnumerateFiles(nutdataRoot, "nutdatapack.ndp", SearchOption.AllDirectories)
            .Where(path =>
            {
                var directoryName = Path.GetFileName(Path.GetDirectoryName(path));
                return directoryName is not null && names.Contains(directoryName);
            })
            .Order(StringComparer.OrdinalIgnoreCase)
            .SelectMany(NdpReader.ReadFile)
            .GroupBy(entry => (entry.Id, entry.FileName))
            .Select(group => group.First())
            .OrderBy(entry => entry.Id)
            .ThenBy(entry => entry.FileName, StringComparer.Ordinal)
            .ToArray();
    }
}
