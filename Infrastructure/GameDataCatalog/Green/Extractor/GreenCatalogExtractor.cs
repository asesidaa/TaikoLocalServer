using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Merging;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public static class GreenCatalogExtractor
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
        var toneEntries = ReadNamedPacks(options.GameDataPath, "tone_name");
        var rewardTitleIds = File.Exists(rewardTitleFiltering)
            ? await BoostXmlReader.ReadRewardTitleIdsAsync(rewardTitleFiltering, cancellationToken)
            : [];

        var overrides = await OverridesLoader.LoadAsync(options.OverridesPath, cancellationToken);
        _ = await new EbootStringResolver().ResolveAsync(options.EbootPath, cancellationToken);
        _ = await new WikiScraper().ResolveAsync(options.UseWiki, cancellationToken);

        var don3d = Don3dDirScanner.Scan(options.GameDataPath);
        var costumes = CostumeMerger.Merge(cosEntries, don3d, overrides);
        var titles = TitleMerger.Merge(titleEntries, rewardTitleIds, overrides);
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
