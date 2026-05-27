using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueCatalogContractTests
{
    [Fact]
    public void BlueRequiredDataPaths_UseBlueEraAndS10100Directory()
    {
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "blue", "data", "config", "S10100-1", "musicinfo.xml"),
            BlueGameDataPaths.MusicInfoXml,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "blue", "data", "config", "S10100-1", "musicmedleyinfo.xml"),
            BlueGameDataPaths.MusicMedleyInfoXml,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "blue", "data", "fumen", "tuning.bin"),
            BlueGameDataPaths.TuningBin,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("S11100-1", BlueGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("green", BlueGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BlueRequiredDataFiles_ListsOnlyNormalCatalogInputs()
    {
        var paths = BlueRequiredDataFiles.GetRequiredPaths();

        Assert.Equal(3, paths.Count);
        Assert.Contains(BlueGameDataPaths.MusicInfoXml, paths);
        Assert.Contains(BlueGameDataPaths.MusicMedleyInfoXml, paths);
        Assert.Contains(BlueGameDataPaths.TuningBin, paths);
        Assert.DoesNotContain(paths, path => path.Contains("battle", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(paths, path => path.Contains("present.xml", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void IBlueCatalog_IsEraCatalog()
    {
        Assert.True(typeof(IEraGameDataCatalog).IsAssignableFrom(typeof(IBlueCatalog)));
    }
}
