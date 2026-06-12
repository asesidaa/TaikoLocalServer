using System.Text.Encodings.Web;
using System.Text.Json;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

internal static class Ac15CustomizationCatalogExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public static async Task ExtractAsync(
        string gameDataRoot,
        string outputDirectory,
        string costumeFileName,
        string titleFileName,
        string neiroFileName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(gameDataRoot))
        {
            throw new DirectoryNotFoundException($"AC15 game-data root does not exist: {gameDataRoot}");
        }

        await WriteAsync(
            outputDirectory,
            costumeFileName,
            Ac15CustomizationSourceParser.BuildCostumes(gameDataRoot),
            cancellationToken);
        await WriteAsync(
            outputDirectory,
            titleFileName,
            Ac15CustomizationSourceParser.BuildTitles(gameDataRoot),
            cancellationToken);
        await WriteAsync(
            outputDirectory,
            neiroFileName,
            Ac15CustomizationSourceParser.BuildNeiros(gameDataRoot),
            cancellationToken);
    }

    private static async Task WriteAsync<T>(
        string outputDirectory,
        string fileName,
        IReadOnlyList<T> items,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, fileName);
        var tempPath = Path.Combine(outputDirectory, $"{fileName}.{Guid.NewGuid():N}.tmp");
        var envelope = new CatalogEnvelope<T>
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

    private sealed class CatalogEnvelope<T>
    {
        public int SchemaVersion { get; init; } = 1;

        public IReadOnlyList<T> Items { get; init; } = [];
    }
}
