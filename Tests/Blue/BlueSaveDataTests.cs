namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueSaveDataTests
{
    [Fact]
    public void CreateDefaultBlueSaveData_InitializesFixedWidthBytesAndProfileDefaults()
    {
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(123);

        Assert.Equal(123u, save.Baid);
        Assert.Equal(string.Empty, save.Title);
        Assert.Equal(0u, save.TitleplateId);
        Assert.Equal(0u, save.ColorFace);
        Assert.Equal(1u, save.ColorBody);
        Assert.Equal(3u, save.ColorLimb);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg1.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg2.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg3.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg4.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, save.CostumeFlg5.Length);
        Assert.True(BitIsSet(save.CostumeFlg1, 0));
        Assert.True(BitIsSet(save.CostumeFlg2, 0));
        Assert.True(BitIsSet(save.CostumeFlg3, 0));
        Assert.True(BitIsSet(save.CostumeFlg4, 0));
        Assert.True(BitIsSet(save.CostumeFlg5, 0));
        Assert.Equal(BlueProtocolBytes.ToneFlagBytes, save.ToneFlg.Length);
        Assert.True(BitIsSet(save.ToneFlg, 0));
        Assert.Equal(BlueProtocolBytes.TitleFlagBytes, save.TitleFlg.Length);
        Assert.Equal(2, save.DefaultOptionSetting.Length);
        Assert.Equal(1u, save.DispDanType);
        Assert.Equal(0u, save.GotDanMax);
        Assert.Equal(BlueProtocolBytes.DanFlagBytes, save.GotDanFlg.Length);
        Assert.Equal(BlueProtocolBytes.DanExtraFlagBytes, save.GotDanExtraFlg.Length);
        Assert.Equal(0u, save.DispTaikojukuDan);
        Assert.True(save.IsAutoCostumeOn);
        Assert.False(save.IsDevil);
        Assert.False(save.IsExplain);
        Assert.False(save.IsChallengeCompe);
        Assert.True(save.IsTojiru);
        Assert.Equal(DateTime.UnixEpoch, save.LastPlayDatetime);
    }

    [Fact]
    public void DefaultBlueSaveData_InitializesReleaseSongFlags()
    {
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);

        Assert.Equal(BlueProtocolBytes.SongFlagBytes, save.ReleaseSongFlg.Length);
        Assert.All(save.ReleaseSongFlg, value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task TaikoDbContext_CanPersistBlueSaveData()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(5));
        await fixture.Context.SaveChangesAsync();

        var loaded = await fixture.Context.UserSaveDataBlue.FindAsync(5u);

        Assert.NotNull(loaded);
        Assert.Equal(5u, loaded!.Baid);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
