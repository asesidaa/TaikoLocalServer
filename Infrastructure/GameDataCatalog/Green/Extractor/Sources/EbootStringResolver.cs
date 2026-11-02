namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

public sealed class EbootStringResolver
{
    public Task<IReadOnlyDictionary<string, string>> ResolveAsync(
        string? ebootPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());
    }
}
