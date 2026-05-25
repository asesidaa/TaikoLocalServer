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
        Assert.Equal(1u, save.DispDanType);
        Assert.Equal((uint)0, save.DispTaikojukuDan);
        Assert.True(save.IsTojiru);
        Assert.Equal(GreenProtocolBytes.GhostReleaseInfoBytes, save.GhostReleaseInfoFlag.Length);
        Assert.Equal(GreenProtocolBytes.GhostPlayedSongBytes, save.GhostPlayedSongFlag.Length);
    }

    [Fact]
    public void CreateDefaultGreenSaveData_DefaultsAutoCostumeOn()
    {
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(123);

        Assert.True(save.IsAutoCostumeOn);
    }
}
