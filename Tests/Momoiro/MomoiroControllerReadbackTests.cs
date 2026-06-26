using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroControllerReadbackTests
{
    [Fact]
    public async Task BaidController_ReturnsMomoiroIdentityFromSharedIdentityAndMomoiroSave()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(22, "22222222222222222222", "MOMO");
        await fixture.SeedMomoiroSaveAsync(22, rewardPtn: 4);
        using var provider = fixture.BuildServiceProvider();

        var response = await InvokeActionAsync<BaidController, BAIDResponse>(
            provider,
            nameof(BaidController.BaidCheck),
            new BAIDRequest
            {
                AccessCode = "22222222222222222222",
                ChassisId = "chassis",
                ShopId = "shop",
                CountryId = "JPN"
            });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeBaid());
        Assert.Equal(22u, response.Baid.GetValueOrDefault());
        Assert.True(response.ShouldSerializePlayerType());
        Assert.Equal(0u, response.PlayerType.GetValueOrDefault());
        Assert.True(response.ShouldSerializeMydonName());
        Assert.Equal("MOMO", response.MydonName);
        Assert.True(response.ShouldSerializeRewardPtn());
        Assert.Equal(4u, response.RewardPtn.GetValueOrDefault());
    }

    [Fact]
    public async Task MyDonEntryController_CreatesMomoiroSaveAndReturnsIdentityFields()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        using var provider = fixture.BuildServiceProvider();

        var response = await InvokeActionAsync<MyDonEntryController, MydonEntryResponse>(
            provider,
            nameof(MyDonEntryController.MydonEntry),
            new MydonEntryRequest
            {
                AccessCode = "55555555555555555555",
                ChassisId = "chassis",
                ShopId = "shop",
                CountryId = "JPN",
                MydonName = "ENTRY",
                RewardPtn = 6
            });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeBaid());
        Assert.Equal(1u, response.Baid.GetValueOrDefault());
        Assert.True(response.ShouldSerializeAccessCode());
        Assert.Equal("55555555555555555555", response.AccessCode);
        Assert.True(response.ShouldSerializeMydonName());
        Assert.Equal("ENTRY", response.MydonName);
        Assert.True(response.ShouldSerializeRewardPtn());
        Assert.Equal(6u, response.RewardPtn.GetValueOrDefault());
        Assert.True(await fixture.MomoiroSaveExistsAsync(1));
    }

    [Fact]
    public async Task SelfBestController_ReturnsMomoiroNormalUraAndShinRows()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(7, "77777777777777777777");
        await fixture.SeedMomoiroSaveAsync(7);
        await fixture.SeedMomoiroBestAsync(7, 101, Difficulty.Oni, false, 345_678, 88, CrownType.Gold);
        await fixture.SeedMomoiroBestAsync(7, 101, Difficulty.UraOni, false, 456_789, 90, CrownType.Clear);
        await fixture.SeedMomoiroBestAsync(7, 101, Difficulty.Oni, true, 567_890, 92, CrownType.Dondaful);
        await fixture.SeedMomoiroBestAsync(7, 101, Difficulty.UraOni, true, 678_901, 94, CrownType.Gold);
        using var provider = fixture.BuildServiceProvider();

        var response = await InvokeActionAsync<SelfBestController, SelfBestResponse>(
            provider,
            nameof(SelfBestController.SelfBest),
            new SelfBestRequest
            {
                Baid = 7,
                ChassisId = "chassis",
                Level = 4,
                ArySongNoes = [101]
            });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeLevel());
        Assert.Equal(4u, response.Level.GetValueOrDefault());
        var normal = Assert.Single(response.ArySelfbestScores);
        Assert.Equal(101u, normal.SongNo);
        Assert.Equal(345_678u, normal.SelfBestScore);
        Assert.Equal(456_789u, normal.UraBestScore);
        var shin = Assert.Single(response.AryShinSelfbestScores);
        Assert.Equal(101u, shin.SongNo);
        Assert.Equal(567_890u, shin.SelfBestScore);
        Assert.Equal(678_901u, shin.UraBestScore);
    }

    [Fact]
    public async Task UserDataController_ReturnsMomoiroListsSongHashReleaseAndCrownBytes()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(8, "88888888888888888888");
        await fixture.SeedMomoiroSaveAsync(
            8,
            releaseSongNoes: [MomoiroHandlerFixture.HighSongNo],
            isDevil: true,
            isExplain: true);
        await fixture.SeedMomoiroFavoriteAsync(8, 250, 0);
        await fixture.SeedMomoiroFavoriteAsync(8, 101, 1);
        await fixture.SeedMomoiroRecentAsync(8, 101, new DateTime(2026, 6, 26, 10, 0, 0));
        await fixture.SeedMomoiroRecentAsync(8, 250, new DateTime(2026, 6, 26, 11, 0, 0));
        await fixture.SeedMomoiroBestAsync(
            8,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.Oni,
            false,
            765_432,
            95,
            CrownType.Gold);
        using var provider = fixture.BuildServiceProvider();

        var response = await InvokeActionAsync<UserDataController, UserDataResponse>(
            provider,
            nameof(UserDataController.UserData),
            new UserDataRequest
            {
                Baid = 8,
                ChassisId = "chassis"
            });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeSongHashVer());
        Assert.Equal(538_116_869u, response.SongHashVer.GetValueOrDefault());
        Assert.Equal([250u, 101u], response.AryFavoriteSongNoes);
        Assert.Equal([250u, 101u], response.AryRecentSongNoes);
        Assert.True(response.ShouldSerializeHashReleaseSongFlg());
        Assert.Equal(48, response.HashReleaseSongFlg.Length);
        Assert.True(BitIsSetByOrdinal(response.HashReleaseSongFlg, MomoiroHandlerFixture.HighSongOrdinal));
        Assert.True(response.ShouldSerializeHashCrownFlg());
        Assert.Equal(475, response.HashCrownFlg.Length);
        Assert.Equal(
            Ac15ProtocolBytes.BuildCrownValue(
                Ac15CrownState.None,
                Ac15CrownState.None,
                Ac15CrownState.None,
                Ac15CrownState.FullCombo,
                Ac15CrownState.None),
            ReadTenBitValue(response.HashCrownFlg, MomoiroHandlerFixture.HighSongOrdinal));
        Assert.True(response.ShouldSerializeIsDevil());
        Assert.True(response.IsDevil.GetValueOrDefault());
        Assert.True(response.ShouldSerializeIsExplain());
        Assert.True(response.IsExplain.GetValueOrDefault());
    }

    private static async Task<TResponse> InvokeActionAsync<TController, TResponse>(
        IServiceProvider services,
        string actionName,
        object request)
        where TController : ControllerBase
    {
        var controller = ActivatorUtilities.CreateInstance<TController>(services);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services }
        };

        var method = typeof(TController).GetMethod(actionName, [request.GetType()]);
        Assert.NotNull(method);

        var result = method.Invoke(controller, [request]);
        if (result is Task<IActionResult> task)
        {
            result = await task;
        }
        else if (result is ValueTask<IActionResult> valueTask)
        {
            result = await valueTask;
        }

        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<TResponse>(ok.Value);
    }

    private static bool BitIsSetByOrdinal(byte[] source, int ordinal)
        => (source[ordinal >> 3] & (1 << (ordinal & 7))) != 0;

    private static ushort ReadTenBitValue(byte[] packed, int index)
    {
        ushort value = 0;
        var bitOffset = index * 10;
        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= (ushort)(1 << bit);
            }
        }

        return value;
    }
}
