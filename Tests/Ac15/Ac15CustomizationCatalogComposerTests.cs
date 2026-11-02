using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CustomizationCatalogComposerTests
{
    private static Title T(uint id, string name = "") => new() { TitleId = id, TitleName = name };

    [Fact]
    public void Compose_WithNoTitlesOfItsOwn_UsesTheSharedList_NotNijiiros()
    {
        var shared = new Dictionary<uint, Title> { [100] = T(100, "ベストスコア 100回更新") };
        var nijiiro = new Dictionary<uint, Title> { [5] = T(5, "Nijiiro title") };

        var catalog = Ac15CustomizationCatalogComposer.Compose([], new Dictionary<uint, Title>(), new Dictionary<uint, Neiro>(),
            [], shared, new Dictionary<uint, Neiro>(), fallbackTitles: nijiiro);

        Assert.Equal([100u], catalog.Titles.Keys);
        Assert.Equal("ベストスコア 100回更新", catalog.Titles[100].TitleName);
    }

    [Fact]
    public void Compose_WithItsOwnTitles_KeepsOnlyThoseIds()
    {
        var shared = new Dictionary<uint, Title> { [1] = T(1, "One"), [2] = T(2, "Two") };

        var catalog = Ac15CustomizationCatalogComposer.Compose([], new Dictionary<uint, Title> { [1] = T(1) }, new Dictionary<uint, Neiro>(),
            [], shared, new Dictionary<uint, Neiro>());

        Assert.Equal([1u], catalog.Titles.Keys);
        Assert.Equal("One", catalog.Titles[1].TitleName);
    }
}
