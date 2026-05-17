using System.Text.Json;
using System.Text.Encodings.Web;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

public static class CatalogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public static async Task WriteAsync<T>(
        string outputDirectory,
        string fileName,
        IReadOnlyList<T> items,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, fileName);
        await using var stream = File.Create(path);
        var envelope = new GreenCatalogEnvelope<T>
        {
            Items = items
        };
        await JsonSerializer.SerializeAsync(stream, envelope, JsonOptions, cancellationToken);
    }
}
