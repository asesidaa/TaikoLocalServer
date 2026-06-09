using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowPersistenceBoundaryTests
{
    [Fact]
    public async Task GetOrCreateYellowSaveData_CreatesDefaultYellowStateWithAc15Lengths()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "YELLOW" });
        await fixture.Context.SaveChangesAsync();

        var save = await fixture.Context.GetOrCreateYellowSaveDataAsync(1);
        await fixture.Context.SaveChangesAsync();

        var limits = Ac15EraProfiles.Yellow.Limits;
        Assert.Equal(1u, save.Baid);
        Assert.Equal(0u, save.TitleplateId);
        Assert.Equal(0u, save.ColorFace);
        Assert.Equal(1u, save.ColorBody);
        Assert.Equal(3u, save.ColorLimb);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg1.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg2.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg3.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg4.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg5.Length);
        Assert.True((save.CostumeFlg1[0] & 1) != 0);
        Assert.Equal(limits.ToneFlagBytes, save.ToneFlg.Length);
        Assert.True((save.ToneFlg[0] & 1) != 0);
        Assert.Equal(limits.TitleFlagBytes, save.TitleFlg.Length);
        Assert.Equal(limits.SongFlagBytes, save.ReleaseSongFlg.Length);
        Assert.Equal(limits.DanFlagBytes, save.GotDanFlg.Length);
        Assert.Equal(limits.DanExtraFlagBytes, save.GotDanExtraFlg.Length);
        Assert.True(save.IsAutoCostumeOn);
        Assert.True(save.IsTojiru);
        Assert.Equal(DateTime.UnixEpoch, save.LastPlayDatetime);
    }

    [Fact]
    public void YellowShopModel_UsesYellowOwnedCompositeKeys()
    {
        using var context = new TaikoDbContext(new DbContextOptionsBuilder<TaikoDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options);

        var season = context.Model.FindEntityType("TaikoLocalServer.Domain.Entities.YellowShopSeasonState");
        var item = context.Model.FindEntityType("TaikoLocalServer.Domain.Entities.YellowShopItemState");

        Assert.NotNull(season);
        Assert.NotNull(item);
        Assert.Equal(["Baid", "SeasonId"], season!.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Equal(["Baid", "SeasonId", "ItemType", "ItemId"], item!.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Equal("YellowShopSeasonStates", season.GetTableName());
        Assert.Equal("YellowShopItemStates", item.GetTableName());
    }

}
