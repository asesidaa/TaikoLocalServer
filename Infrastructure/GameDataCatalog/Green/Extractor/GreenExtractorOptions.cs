namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public sealed record GreenExtractorOptions(
    string GameDataPath,
    string OutputDirectory,
    string? EbootPath = null,
    bool UseWiki = false,
    string? OverridesPath = null);
