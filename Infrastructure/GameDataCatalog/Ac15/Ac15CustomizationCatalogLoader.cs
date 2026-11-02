using System.Text.Json;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

internal static class Ac15CustomizationCatalogLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<IReadOnlyList<T>> LoadListAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        await using var stream = File.OpenRead(path);
        var envelope = await JsonSerializer.DeserializeAsync<CatalogEnvelope<T>>(stream, JsonOptions, cancellationToken)
                       ?? new CatalogEnvelope<T>();
        return envelope.Items;
    }

    private sealed class CatalogEnvelope<T>
    {
        public int SchemaVersion { get; init; } = 1;

        public IReadOnlyList<T> Items { get; init; } = [];
    }
}
