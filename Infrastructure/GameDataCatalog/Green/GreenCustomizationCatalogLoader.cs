using System.Text.Json;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal static class GreenCustomizationCatalogLoader
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
        var envelope = await JsonSerializer.DeserializeAsync<GreenCatalogEnvelope<T>>(stream, JsonOptions, cancellationToken)
                       ?? new GreenCatalogEnvelope<T>();
        return envelope.Items;
    }
}
