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

        var cosEntries = File.Exists(cosNamePack) ? NdpReader.ReadFile(cosNamePack) : [];
        var titleEntries = File.Exists(titleNamePack) ? NdpReader.ReadFile(titleNamePack) : [];
        var toneEntries = File.Exists(toneNamePack) ? NdpReader.ReadFile(toneNamePack) : [];
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
}
