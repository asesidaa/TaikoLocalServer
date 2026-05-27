namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueDocsTests
{
    [Fact]
    public void RootReadme_DocumentsBlueDataLayout()
    {
        var source = File.ReadAllText(FindRepoFile("README.md"));

        Assert.Contains("wwwroot/data/blue/data/config/S10100-1/musicinfo.xml", source, StringComparison.Ordinal);
        Assert.Contains("wwwroot/data/blue/data/config/S10100-1/musicmedleyinfo.xml", source, StringComparison.Ordinal);
        Assert.Contains("wwwroot/data/blue/data/fumen/tuning.bin", source, StringComparison.Ordinal);
    }

    [Fact]
    public void HostReadme_DocumentsBlueSymlinkAndBattleDeferral()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "README.md"));

        Assert.Contains("Blue AC15 Test Support", source, StringComparison.Ordinal);
        Assert.Contains("S10100-1", source, StringComparison.Ordinal);
        Assert.Contains("wwwroot/data/blue/data", source, StringComparison.Ordinal);
        Assert.Contains("blue_item_shop_data.json", source, StringComparison.Ordinal);
        Assert.Contains("config/S10100-1/battle", source, StringComparison.Ordinal);
        Assert.Contains("Track B", source, StringComparison.Ordinal);
    }

    private static string FindRepoFile(params string[] pathParts)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Could not find {Path.Combine(pathParts)}.");
    }
}
