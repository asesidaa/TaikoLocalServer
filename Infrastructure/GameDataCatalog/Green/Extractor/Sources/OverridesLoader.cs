using System.Text.Json;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Sources;

public sealed class GreenCatalogOverrides
{
    public IReadOnlyDictionary<uint, GreenCostumeOverride> Costumes { get; init; } = new Dictionary<uint, GreenCostumeOverride>();
    public IReadOnlyDictionary<uint, GreenNamedOverride> Titles { get; init; } = new Dictionary<uint, GreenNamedOverride>();
    public IReadOnlyDictionary<uint, GreenNamedOverride> Neiros { get; init; } = new Dictionary<uint, GreenNamedOverride>();
}

public sealed class GreenCostumeOverride
{
    public string? Name { get; init; }
    public string? CostumeType { get; init; }
}

public sealed class GreenNamedOverride
{
    public string? Name { get; init; }
}

public static class OverridesLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<GreenCatalogOverrides> LoadAsync(
        string? path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new GreenCatalogOverrides();
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<GreenCatalogOverrides>(stream, JsonOptions, cancellationToken)
               ?? new GreenCatalogOverrides();
    }
}
