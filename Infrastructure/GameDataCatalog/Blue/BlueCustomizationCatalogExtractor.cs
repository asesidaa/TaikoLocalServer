using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal static class BlueCustomizationCatalogExtractor
{
    public static Task ExtractAsync(
        string gameDataRoot,
        string outputDirectory,
        CancellationToken cancellationToken)
        => Ac15CustomizationCatalogExtractor.ExtractAsync(
            gameDataRoot,
            outputDirectory,
            BlueCostumeLoader.FileName,
            BlueTitleLoader.FileName,
            BlueNeiroLoader.FileName,
            cancellationToken);
}
