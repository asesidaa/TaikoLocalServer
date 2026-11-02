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
        var tempPath = Path.Combine(outputDirectory, $"{fileName}.{Guid.NewGuid():N}.tmp");
        var envelope = new GreenCatalogEnvelope<T>
        {
            Items = items
        };

        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, envelope, JsonOptions, cancellationToken);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
