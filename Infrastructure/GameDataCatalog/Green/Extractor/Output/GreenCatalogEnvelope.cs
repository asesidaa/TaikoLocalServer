namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

public sealed class GreenCatalogEnvelope<T>
{
    public int SchemaVersion { get; init; } = 1;

    public IReadOnlyList<T> Items { get; init; } = [];
}
