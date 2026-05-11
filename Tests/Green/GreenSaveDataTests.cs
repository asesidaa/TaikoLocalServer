namespace TaikoLocalServer.Tests.Green;

public sealed class GreenSaveDataTests
{
    [Fact]
    public void CreateDefaultGreenSaveData_InitializesFixedWidthBytes()
    {
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(123);

        Assert.Equal((uint)123, save.Baid);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg1.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg2.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg3.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg4.Length);
        Assert.Equal(GreenProtocolBytes.CostumeFlagBytes, save.CostumeFlg5.Length);
        Assert.Equal(GreenProtocolBytes.ToneFlagBytes, save.ToneFlg.Length);
        Assert.Equal(GreenProtocolBytes.TitleFlagBytes, save.TitleFlg.Length);
        Assert.Equal(2, save.DefaultOptionSetting.Length);
        Assert.Equal(GreenProtocolBytes.DanFlagBytes, save.GotDanFlg.Length);
        Assert.Equal(GreenProtocolBytes.DanExtraFlagBytes, save.GotDanExtraFlg.Length);
        Assert.Equal(GreenProtocolBytes.GhostReleaseInfoBytes, save.GhostReleaseInfoFlag.Length);
        Assert.Equal(GreenProtocolBytes.GhostPlayedSongBytes, save.GhostPlayedSongFlag.Length);
    }

    [Fact]
    public void CreateFakeBestSeeds_UsesCatalogOrder()
    {
        GreenMusicInfoEntry[] songs =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 },
            new() { SongNo = 104, MusicId = "d", FileOrder = 3 },
            new() { SongNo = 105, MusicId = "e", FileOrder = 4, HasExtreme = true }
        ];

        var seeds = GreenSeedDataService.CreateFakeBestSeeds(55, songs).ToArray();

        Assert.Equal(5, seeds.Length);
        Assert.Equal((uint)55, seeds[0].Baid);
        Assert.Equal((uint)101, seeds[0].SongId);
        Assert.Equal(Difficulty.Easy, seeds[0].Difficulty);
        Assert.Equal(CrownType.Clear, seeds[0].BestCrown);
        Assert.Equal(Difficulty.UraOni, seeds[4].Difficulty);
    }

    [Fact]
    public void GrantFirstFakeDanIfNeeded_GrantsOnlyOnce()
    {
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);

        var first = GreenSeedDataService.GrantFirstFakeDanIfNeeded(save);
        var second = GreenSeedDataService.GrantFirstFakeDanIfNeeded(save);

        Assert.True(first);
        Assert.False(second);
        Assert.Equal((uint)1, save.GotDanMax);
        Assert.Equal((uint)1, save.DispTaikojukuDan);
        Assert.Equal(0b0000_0001, save.GotDanFlg[0]);
    }
}
