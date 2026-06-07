using TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowCatalogContractTests
{
    [Fact]
    public void YellowRequiredDataPaths_UseYellowEraAndST9100Directory()
    {
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "yellow", "data", "config", "ST9100-1", "musicinfo.xml"),
            YellowGameDataPaths.MusicInfoXml,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "yellow", "data", "config", "ST9100-1", "musicmedleyinfo.xml"),
            YellowGameDataPaths.MusicMedleyInfoXml,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "yellow", "data", "config", "ST9100-1", "defmusic.bin"),
            YellowGameDataPaths.DefMusicBin,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("wwwroot", "data", "yellow", "data", "fumen", "tuning.bin"),
            YellowGameDataPaths.TuningBin,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("S10100-1", YellowGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("S11100-1", YellowGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("blue", YellowGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("green", YellowGameDataPaths.MusicInfoXml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void YellowRequiredDataFiles_ListsOnlyPhase13CatalogInputs()
    {
        var paths = YellowRequiredDataFiles.GetRequiredPaths();

        Assert.Equal(4, paths.Count);
        Assert.Contains(YellowGameDataPaths.MusicInfoXml, paths);
        Assert.Contains(YellowGameDataPaths.MusicMedleyInfoXml, paths);
        Assert.Contains(YellowGameDataPaths.DefMusicBin, paths);
        Assert.Contains(YellowGameDataPaths.TuningBin, paths);
        Assert.DoesNotContain(paths, path => path.Contains("battle", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(paths, path => path.Contains("present.xml", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(paths, path => path.Contains("yellow_telop_data.json", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(paths, path => path.Contains("yellow_event_folder_data.json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void IYellowCatalog_IsEraCatalog()
    {
        Assert.True(typeof(IEraGameDataCatalog).IsAssignableFrom(typeof(IYellowCatalog)));
    }
}
