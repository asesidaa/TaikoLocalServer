using System.Text.Json;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCustomizationCatalogLoaderTests
{
    [Fact]
    public async Task CostumeLoader_MissingFileReturnsEmptyList()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var items = await GreenCostumeLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Empty(items);
    }

    [Fact]
    public async Task TitleLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new GreenCatalogEnvelope<Title>
        {
            Items =
            [
                new Title { TitleId = 132, TitleName = "B" },
                new Title { TitleId = 131, TitleName = "A" }
            ]
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var items = await GreenTitleLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal(new uint[] { 131, 132 }, items.Keys.OrderBy(id => id));
        File.Delete(path);
    }

    [Fact]
    public async Task NeiroLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new GreenCatalogEnvelope<Neiro>
        {
            Items =
            [
                new Neiro { NeiroId = 4, NeiroName = "Tone" }
            ]
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var items = await GreenNeiroLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal("Tone", items[4].NeiroName);
        File.Delete(path);
    }
}
