using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;
using BlueBalanceCheckController = TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers.BalanceCheckController;
using BlueBaidResponseMapper = TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers.BaidResponseMapper;
using BlueBanacoinPaymentController = TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers.BanacoinPaymentController;
using BlueGetBanacoinInfoController = TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers.GetBanacoinInfoController;
using BlueUserDataMappers = TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers.UserDataMappers;
using BlueWire = TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
using YellowBaidResponseMapper = TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers.BaidResponseMapper;
using YellowUserDataMappers = TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers.UserDataMappers;
using YellowWire = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowCompatibilityResponseShapeTests
{
    [Fact]
    public void BalanceCheck_YellowSerializesFieldsBlueSends()
    {
        var blue = Invoke(
            new BlueBalanceCheckController(),
            controller => controller.BalanceCheck(new BlueWire.BalancecheckRequest { Personid = "person" }),
            value => Assert.IsType<BlueWire.BalancecheckResponse>(value));
        var yellow = Invoke(
            new BalanceCheckController(),
            controller => controller.BalanceCheck(new YellowWire.BalancecheckRequest { Personid = "person" }),
            value => Assert.IsType<YellowWire.BalancecheckResponse>(value));

        AssertYellowIncludesBlueSerializedFields(blue, yellow);
        Assert.Equal(blue.Result, yellow.Result);
        Assert.Equal(blue.Personid, yellow.Personid);
        Assert.Equal(blue.BnidResult, yellow.BnidResult);
        Assert.Equal(blue.CoinCoupon, yellow.CoinCoupon);
    }

    [Fact]
    public void BanacoinPayment_YellowSerializesFieldsBlueSends()
    {
        var blue = Invoke(
            new BlueBanacoinPaymentController(),
            controller => controller.BanacoinPayment(new BlueWire.BanacoinpaymentRequest { Personid = "person" }),
            value => Assert.IsType<BlueWire.BanacoinpaymentResponse>(value));
        var yellow = Invoke(
            new BanacoinPaymentController(),
            controller => controller.BanacoinPayment(new YellowWire.BanacoinpaymentRequest { Personid = "person" }),
            value => Assert.IsType<YellowWire.BanacoinpaymentResponse>(value));

        AssertYellowIncludesBlueSerializedFields(blue, yellow);
        Assert.Equal(blue.Result, yellow.Result);
        Assert.Equal(blue.Personid, yellow.Personid);
        Assert.Equal(blue.BnidResult, yellow.BnidResult);
        Assert.Equal(blue.Chid, yellow.Chid);
    }

    [Fact]
    public void GetBanacoinInfo_YellowKeepsBlueResultOnlyShape()
    {
        var blue = Invoke(
            new BlueGetBanacoinInfoController(),
            controller => controller.GetBanacoinInfo(new BlueWire.GetbanacoininfoRequest()),
            value => Assert.IsType<BlueWire.GetbanacoininfoResponse>(value));
        var yellow = Invoke(
            new GetBanacoinInfoController(),
            controller => controller.GetBanacoinInfo(new YellowWire.GetbanacoininfoRequest()),
            value => Assert.IsType<YellowWire.GetbanacoininfoResponse>(value));

        Assert.Equal(blue.Result, yellow.Result);
        AssertSameSharedFieldPresence(blue, yellow);
    }

    [Fact]
    public async Task RewardCardCheck_YellowSerializesBaidLikeBlue()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 42, MyDonName = "DON" });
        fixture.Context.Cards.Add(new Card { AccessCode = "12345678901234567890", Baid = 42 });
        await fixture.Context.SaveChangesAsync();

        var controller = new RewardCardCheckController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(fixture.Context)
            }
        };

        var result = await controller.RewardCardCheck(new YellowWire.RewardcardcheckRequest
        {
            AccessCode = "12345678901234567890",
            ChassisId = "268410000000"
        });
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<YellowWire.RewardcardcheckResponse>(ok.Value);

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeBaid());
        Assert.Equal(42u, response.Baid);
    }

    [Fact]
    public void BaidResponse_YellowSerializesEveryBlueExistingUserField()
    {
        var common = CreateRepresentativeBaidResponse();
        var blue = AssembleBlueBaidControllerShape(common);
        var yellow = AssembleYellowBaidControllerShape(common);

        AssertYellowIncludesBlueSerializedFields(blue, yellow);
    }

    [Fact]
    public void UserDataMapper_YellowTokkunResponseSerializesEveryBlueField()
    {
        var common = CreateRepresentativeTokkunUserDataResponse();
        var blue = AssembleBlueUserDataControllerShape(common);
        var yellow = YellowUserDataMappers.Map(common);

        AssertYellowIncludesBlueSerializedFields(blue, yellow);
        Assert.True(yellow.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(blue.TokkunTutorialFlg, yellow.TokkunTutorialFlg);
    }

    private static TResponse Invoke<TController, TResponse>(
        TController controller,
        Func<TController, IActionResult> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = CreateHttpContext()
        };
        var ok = Assert.IsType<OkObjectResult>(action(controller));
        return assertResponse(ok.Value);
    }

    private static DefaultHttpContext CreateHttpContext(ITaikoDbContext? context = null)
    {
        var serviceCollection = new ServiceCollection()
            .AddLogging();

        if (context is not null)
        {
            serviceCollection
                .AddApplication()
                .AddSingleton(context);
        }

        return new DefaultHttpContext
        {
            RequestServices = serviceCollection.BuildServiceProvider()
        };
    }

    private static Ac15BaidResponse CreateRepresentativeBaidResponse()
    {
        var profile = new Ac15BaidProfile
        {
            Title = "Title",
            TitlePlateId = 3,
            ColorFace = 4,
            ColorBody = 5,
            ColorLimb = 6,
            SelectedCostume = new Ac15CostumeFacts(1, 2, 3, 4, 5),
            IsAutoCostumeOn = true,
            LastPlayDatetime = "20260608120000",
            DefaultToneSetting = 13
        };
        var costumeFlags = new Ac15BaidCostumeFlags(
            new byte[Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes],
            new byte[Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes],
            new byte[Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes],
            new byte[Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes],
            new byte[Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes]);
        var shopMedals = new Ac15BaidShopMedals(7, 8, 9, 10, 11);
        var dan = new Ac15BaidDan(
            1,
            12,
            new byte[Ac15EraProfiles.Yellow.Limits.DanFlagBytes],
            new byte[Ac15EraProfiles.Yellow.Limits.DanExtraFlagBytes]);
        var compatibility = new Ac15BaidCompatibility("1", 14);

        return new Ac15BaidResponse
        {
            Result = 1,
            Baid = 42,
            Identity = new Ac15BaidIdentity("DON", 0),
            MydonProfile = profile,
            CustomizationInventory = costumeFlags,
            ShopMedalBalance = shopMedals,
            DanStatus = dan,
            CompatibilityProfile = compatibility
        };
    }

    private static Ac15UserDataResponse CreateRepresentativeTokkunUserDataResponse()
        => new()
        {
            Result = 1,
            SongLists = new Ac15UserDataSongLists
            {
                AryFavoriteSongNoes = [101, 102],
                AryRecentSongNoes = [103]
            },
            SongFlags = new Ac15UserDataSongFlags
            {
                SongHashVer = 789,
                ReleaseSongFlg = new byte[32],
                OptionFlg = [1],
                ToneFlg = new byte[32],
                TitleFlg = new byte[32]
            },
            Counters = new Ac15UserDataProfileCounters
            {
                CategJpopCnt = 2,
                CategAnimeCnt = 3,
                CategDoyoCnt = 4,
                CategVarietyCnt = 5,
                CategClassicCnt = 6,
                CategGameCnt = 7,
                CategNamcoCnt = 8,
                CategVocaloidCnt = 9,
                SongPushedCnt = 10,
                SongFavoriteCnt = 11,
                SongRecentCnt = 12,
                TotalCreditCnt = 13,
                PrevAreaCode = 14,
                ConsecAreaCnt = 15
            },
            Recommendations = new Ac15UserDataRecommendations
            {
                RecommendSong = 16,
                RecommendBestSong = [17, 18]
            },
            Display = new Ac15UserDataDisplaySettings
            {
                DispLevelTotal = 19,
                DispLevelChassis = 20,
                DispLevelSelf = 21,
                DefaultOptionSetting = [22, 23],
                DefaultShinSetting = true,
                DispTaikojukuDan = 2,
                DifficultyPlayedCourse = 24,
                DifficultyPlayedStar = 25,
                IsChallengeCompe = true,
                IsTojiru = true
            },
            ModeFlags = new Ac15UserDataModeFlags(true, true),
            Tutorial = new Ac15UserDataTutorial(7, null)
        };

    private static BlueWire.UserDataResponse AssembleBlueUserDataControllerShape(Ac15UserDataResponse common)
    {
        var response = new BlueWire.UserDataResponse
        {
            Result = common.Result
        };

        BlueUserDataMappers.Apply(common.SongFlags, response);
        BlueUserDataMappers.Apply(common.SongLists, response);
        BlueUserDataMappers.Apply(common.Recommendations, response);
        BlueUserDataMappers.Apply(common.Counters, response);
        BlueUserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            BlueUserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            BlueUserDataMappers.Apply(tutorial, response);
        }

        return response;
    }

    private static BlueWire.BAIDResponse AssembleBlueBaidControllerShape(Ac15BaidResponse common)
    {
        var response = new BlueWire.BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            ContentInfo = new byte[BlueProtocolBytes.ContentInfoBytes]
        };

        if (common.Identity is { } identity)
        {
            BlueBaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            BlueBaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            BlueBaidResponseMapper.Apply(inventory, response);
        }

        if (common.ShopMedalBalance is { } medals)
        {
            BlueBaidResponseMapper.Apply(medals, response);
        }

        if (common.DanStatus is { } dan)
        {
            BlueBaidResponseMapper.Apply(dan, response);
        }

        if (common.CompatibilityProfile is { } compatibility)
        {
            BlueBaidResponseMapper.Apply(compatibility, response);
        }

        return ApplyBlueBaidControllerShape(response);
    }

    private static BlueWire.BAIDResponse ApplyBlueBaidControllerShape(BlueWire.BAIDResponse response)
    {
        response.AccessCode = "12345678901234567890";
        response.IsPublish = true;
        response.PlayerType = 0;
        response.ComSvrResult = 1;
        response.Personid = "1";
        response.RegCountryId = "JPN";
        response.MbId = 1;
        response.PurposeId = 1;
        response.RegionId = 1;
        return response;
    }

    private static YellowWire.BAIDResponse AssembleYellowBaidControllerShape(Ac15BaidResponse common)
    {
        var response = new YellowWire.BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            ContentInfo = new byte[Ac15EraProfiles.Yellow.Limits.ContentInfoBytes]
        };

        if (common.Identity is { } identity)
        {
            YellowBaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            YellowBaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            YellowBaidResponseMapper.Apply(inventory, response);
        }

        if (common.ShopMedalBalance is { } medals)
        {
            YellowBaidResponseMapper.Apply(medals, response);
        }

        if (common.DanStatus is { } dan)
        {
            YellowBaidResponseMapper.Apply(dan, response);
        }

        if (common.CompatibilityProfile is { } compatibility)
        {
            YellowBaidResponseMapper.Apply(compatibility, response);
        }

        return ApplyYellowBaidControllerShape(response);
    }

    private static YellowWire.BAIDResponse ApplyYellowBaidControllerShape(YellowWire.BAIDResponse response)
    {
        response.AccessCode = "12345678901234567890";
        response.IsPublish = true;
        response.PlayerType = 0;
        response.ComSvrResult = 1;
        response.Personid = "1";
        response.RegCountryId = "JPN";
        response.MbId = 1;
        response.PurposeId = 1;
        response.RegionId = 1;
        return response;
    }

    private static void AssertYellowIncludesBlueSerializedFields(object blue, object yellow)
    {
        var bluePresence = SerializedFieldPresence(blue);
        var yellowPresence = SerializedFieldPresence(yellow);

        foreach (var (field, isSerialized) in bluePresence.Where(pair => pair.Value))
        {
            Assert.True(
                yellowPresence.TryGetValue(field, out var yellowSerialized),
                $"Yellow {yellow.GetType().Name} does not expose {field}.");
            Assert.True(
                yellowSerialized,
                $"Yellow {yellow.GetType().Name} omitted {field} that Blue serializes.");
        }
    }

    private static void AssertSameSharedFieldPresence(object blue, object yellow)
    {
        var bluePresence = SerializedFieldPresence(blue);
        var yellowPresence = SerializedFieldPresence(yellow);

        foreach (var (field, blueSerialized) in bluePresence)
        {
            if (!yellowPresence.TryGetValue(field, out var yellowSerialized))
            {
                continue;
            }

            Assert.Equal(blueSerialized, yellowSerialized);
        }
    }

    private static Dictionary<string, bool> SerializedFieldPresence(object response)
        => response.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.Name.StartsWith("ShouldSerialize", StringComparison.Ordinal)
                             && method.GetParameters().Length == 0
                             && method.ReturnType == typeof(bool))
            .ToDictionary(
                method => method.Name["ShouldSerialize".Length..],
                method => (bool)method.Invoke(response, null)!);
}
