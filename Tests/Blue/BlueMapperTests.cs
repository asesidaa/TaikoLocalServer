using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using BAIDResponse = TaikoLocalServer.Adapters.GameProtocol.Blue.Wire.BAIDResponse;
using BlueUserDataResponse = TaikoLocalServer.Adapters.GameProtocol.Blue.Wire.UserDataResponse;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueMapperTests
{
    [Fact]
    public void UserDataMapper_Blue_OmitsTokkunTutorialFlag()
    {
        var response = AssembleBlueUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            SongFlags = new Ac15UserDataSongFlags
            {
                ReleaseSongFlg = new byte[BlueProtocolBytes.SongFlagBytes],
                ToneFlg = new byte[BlueProtocolBytes.ToneFlagBytes],
                TitleFlg = new byte[BlueProtocolBytes.TitleFlagBytes]
            },
            Display = new Ac15UserDataDisplaySettings
            {
                DefaultOptionSetting = new byte[2],
                DispTaikojukuDan = 1
            }
        });

        Assert.False(response.ShouldSerializeTokkunTutorialFlg());
    }

    [Fact]
    public void UserDataMapper_Blue_MapsHowToPlayTutorialFlag()
    {
        var response = AssembleBlueUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            ModeFlags = new Ac15UserDataModeFlags(IsDevil: null, IsExplain: true)
        });

        Assert.True(response.ShouldSerializeIsExplain());
        Assert.True(response.IsExplain);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(7u)]
    public void UserDataMapper_Blue_MapsRawTokkunTutorialFlagWhenPresent(uint tokkunTutorialFlg)
    {
        var response = AssembleBlueUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            SongFlags = new Ac15UserDataSongFlags
            {
                ReleaseSongFlg = new byte[BlueProtocolBytes.SongFlagBytes],
                ToneFlg = new byte[BlueProtocolBytes.ToneFlagBytes],
                TitleFlg = new byte[BlueProtocolBytes.TitleFlagBytes]
            },
            Display = new Ac15UserDataDisplaySettings
            {
                DefaultOptionSetting = new byte[2],
                DispTaikojukuDan = 1
            },
            Tutorial = new Ac15UserDataTutorial(tokkunTutorialFlg, DifficultyTutorialFlg: null)
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
        var response = AssembleBlueUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            Display = new Ac15UserDataDisplaySettings { DispTaikojukuDan = dispTaikojukuDan }
        });

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(1u, response.DispTaikojukuDan);
    }

    private static BlueUserDataResponse AssembleBlueUserDataResponse(Ac15UserDataResponse common)
    {
        var response = new BlueUserDataResponse
        {
            Result = common.Result
        };

        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Recommendations, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            UserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            UserDataMappers.Apply(tutorial, response);
        }

        return response;
    }

    [Fact]
    public void BaidMapper_Blue_AppliesFixedWidthProtocolByteSections()
    {
        var response = new BAIDResponse
        {
            Result = 1,
            Baid = 3,
            ContentInfo = new byte[BlueProtocolBytes.ContentInfoBytes]
        };
        BaidResponseMapper.Apply(new Ac15BaidIdentity("DON", 0), response);
        BaidResponseMapper.Apply(new Ac15BaidCostumeFlags([], [], [], [], []), response);
        BaidResponseMapper.Apply(new Ac15BaidDan(0, 0, [], []), response);

        Assert.Equal("DON", response.MydonName);
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
