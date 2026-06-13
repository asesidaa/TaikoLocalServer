using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueMapperTests
{
    [Fact]
    public void UserDataMapper_Blue_OmitsTokkunTutorialFlag()
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            ReleaseSongFlg = new byte[BlueProtocolBytes.SongFlagBytes],
            ToneFlg = new byte[BlueProtocolBytes.ToneFlagBytes],
            TitleFlg = new byte[BlueProtocolBytes.TitleFlagBytes],
            DefaultOptionSetting = new byte[2],
            DispTaikojukuDan = 1
        });

        Assert.False(response.ShouldSerializeTokkunTutorialFlg());
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(7u)]
    public void UserDataMapper_Blue_MapsRawTokkunTutorialFlagWhenPresent(uint tokkunTutorialFlg)
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            ReleaseSongFlg = new byte[BlueProtocolBytes.SongFlagBytes],
            ToneFlg = new byte[BlueProtocolBytes.ToneFlagBytes],
            TitleFlg = new byte[BlueProtocolBytes.TitleFlagBytes],
            DefaultOptionSetting = new byte[2],
            DispTaikojukuDan = 1,
            TokkunTutorialFlg = tokkunTutorialFlg
        });

        Assert.True(response.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(tokkunTutorialFlg, response.TokkunTutorialFlg);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(26u)]
    [InlineData(20001u)]
    public void UserDataMapper_Blue_FallsBackToSentinelOneForInvalidDispTaikojukuDan(uint dispTaikojukuDan)
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            DispTaikojukuDan = dispTaikojukuDan
        });

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(1u, response.DispTaikojukuDan);
    }

    [Fact]
    public void BaidMapper_Blue_UsesBlueFixedWidthFallbacks()
    {
        var response = BaidResponseMapper.Map(new Ac15BaidResponse
        {
            Result = 1,
            Baid = 3,
            Identity = new Ac15BaidIdentity("DON", 0)
        });

        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg1.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg2.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg3.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg4.Length);
        Assert.Equal(BlueProtocolBytes.CostumeFlagBytes, response.CostumeFlg5.Length);
        Assert.Equal(BlueProtocolBytes.DanFlagBytes, response.GotDanFlg.Length);
        Assert.Equal(BlueProtocolBytes.DanExtraFlagBytes, response.GotDanextraFlg.Length);
        Assert.Equal(BlueProtocolBytes.ContentInfoBytes, response.ContentInfo.Length);
    }
}
