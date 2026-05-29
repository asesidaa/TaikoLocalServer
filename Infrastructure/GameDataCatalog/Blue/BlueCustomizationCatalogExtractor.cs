using System.Text.Encodings.Web;
using System.Text.Json;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal static class BlueCustomizationCatalogExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public static async Task ExtractAsync(
        string gameDataRoot,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(gameDataRoot))
        {
            throw new DirectoryNotFoundException($"Blue game-data root does not exist: {gameDataRoot}");
        }

        await WriteAsync(
            outputDirectory,
            BlueCostumeLoader.FileName,
            Ac15CustomizationSourceParser.BuildCostumes(gameDataRoot),
            cancellationToken);
        await WriteAsync(
            outputDirectory,
            BlueTitleLoader.FileName,
            Ac15CustomizationSourceParser.BuildTitles(gameDataRoot),
            cancellationToken);
        await WriteAsync(
            outputDirectory,
            BlueNeiroLoader.FileName,
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
