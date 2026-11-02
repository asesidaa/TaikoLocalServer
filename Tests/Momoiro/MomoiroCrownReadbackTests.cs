using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroCrownReadbackTests
{
    [Fact]
    public async Task UserDataController_EmptyMomoiroSaveReturnsFixedZeroHashCrownFlg()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(30, "30303030303030303030");
        await fixture.SeedMomoiroSaveAsync(30);
        using var provider = fixture.BuildServiceProvider();

        var response = await InvokeActionAsync<UserDataController, UserDataResponse>(
            provider,
            nameof(UserDataController.UserData),
            new UserDataRequest
            {
                Baid = 30,
                ChassisId = "chassis"
            });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeHashCrownFlg());
        Assert.Equal(Ac15EraProfiles.Momoiro.Limits.CrownPackedBytes, response.HashCrownFlg.Length);
        Assert.All(response.HashCrownFlg, value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task UserDataController_HighSongIdCrownPackingUsesMomoiroCatalogOrdinal()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(31, "31313131313131313131");
        await fixture.SeedMomoiroSaveAsync(31);
        await fixture.SeedMomoiroBestAsync(
            31,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.Easy,
            false,
            100_000,
            70,
            CrownType.Clear);
        await fixture.SeedMomoiroBestAsync(
            31,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.Normal,
            false,
            200_000,
            80,
            CrownType.Gold);
        await fixture.SeedMomoiroBestAsync(
            31,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.Oni,
            false,
            300_000,
            90,
            CrownType.Dondaful);
        await fixture.SeedMomoiroBestAsync(
            31,
            MomoiroHandlerFixture.HighSongNo,
            Difficulty.UraOni,
            false,
            400_000,
            95,
            CrownType.Clear);
        using var provider = fixture.BuildServiceProvider();

        var response = await InvokeActionAsync<UserDataController, UserDataResponse>(
            provider,
            nameof(UserDataController.UserData),
            new UserDataRequest
            {
                Baid = 31,
                ChassisId = "chassis"
            });

        var expected = (byte)Ac15ProtocolBytes.BuildCrownValue(
            Ac15CrownState.Clear,
            Ac15CrownState.FullCombo,
            Ac15CrownState.None,
            Ac15CrownState.FullCombo,
            Ac15CrownState.None);

        Assert.True(response.ShouldSerializeHashCrownFlg());
        Assert.Equal(Ac15EraProfiles.Momoiro.Limits.CrownPackedBytes, response.HashCrownFlg.Length);
        Assert.Equal(expected, response.HashCrownFlg[MomoiroHandlerFixture.HighSongOrdinal]);
        Assert.Equal(0, response.HashCrownFlg[MomoiroHandlerFixture.HighSongOrdinal - 1]);
        Assert.True(MomoiroHandlerFixture.HighSongNo < Ac15EraProfiles.Momoiro.Limits.CrownSongCount);
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

}
