using System.Reflection;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Kimidori;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CustomizationSourceParserTests
{
    [Theory]
    [InlineData("murasaki", 399u)]
    [InlineData("kimidori", 320u)]
    public void BuildTitles_ReadsPackedTitleRangesFromLocalOlderAc15Data(
        string era,
        uint expectedHighestTitleId)
    {
        var root = Path.Combine(FindRepoRoot(), "Host", "wwwroot", "data", era, "data");

        var titles = BuildTitles(root);

        Assert.NotEmpty(titles);
        Assert.Contains(titles, title => title.TitleId == expectedHighestTitleId);
    }

    [Fact]
    public void BuildTitles_ReadsPackedOlderAc15TitleNameRanges()
    {
        var root = Path.Combine(Path.GetTempPath(), "TaikoLocalServerTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "nutdata", "pack", "ST6100-1", "00", "title_name"));
        File.WriteAllBytes(
            Path.Combine(root, "nutdata", "pack", "ST6100-1", "00", "title_name", "nutdatapack_00000_00002.ndp"),
            []);

        var titles = BuildTitles(root);

        Assert.Equal(new uint[] { 0, 1, 2 }, titles.Select(title => title.TitleId));
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static IReadOnlyList<Title> BuildTitles(string root)
    {
        var parser = typeof(KimidoriEraGameDataCatalog).Assembly.GetType(
            "TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15.Ac15CustomizationSourceParser")
                     ?? throw new InvalidOperationException("Could not locate AC15 customization source parser.");
        var method = parser.GetMethod("BuildTitles", BindingFlags.Public | BindingFlags.Static)
                     ?? throw new InvalidOperationException("Could not locate BuildTitles.");

        return (IReadOnlyList<Title>)(method.Invoke(null, [root])
            ?? throw new InvalidOperationException("BuildTitles returned null."));
    }
}
