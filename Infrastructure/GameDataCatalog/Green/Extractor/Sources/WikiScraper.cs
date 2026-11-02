namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

public sealed class WikiScraper
{
    public Task<IReadOnlyDictionary<string, string>> ResolveAsync(
        bool enabled,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());
    }
}
