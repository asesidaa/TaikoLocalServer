using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedAdminApiTests
{
    [Fact]
    public async Task UserSettings_Red_ReadsAndSavesRedProfileOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        fixture.Context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(1));
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.Costume1 = 5;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], Ac15EraProfiles.Red.Limits.CostumeFlagBytes);
        save.TitleFlg = BitsetCodec.Encode([10], Ac15EraProfiles.Red.Limits.TitleFlagBytes);
        save.ToneFlg = BitsetCodec.Encode([0, 4], Ac15EraProfiles.Red.Limits.ToneFlagBytes);
        save.DefaultToneSetting = 4;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = true;
        save.IsChallengeCompe = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        fixture.Context.UserSaveDataRed.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateUserSettingsController(fixture.Context);

        var getResult = await controller.GetUserSetting("Red", 1);

        var ok = Assert.IsType<OkObjectResult>(getResult.Result);
        var setting = Assert.IsType<UserSetting>(ok.Value);
        Assert.Equal(5u, setting.Kigurumi);
        Assert.Equal([0u, 5u], setting.UnlockedKigurumi);
        Assert.Equal([10u], setting.UnlockedTitle);
        Assert.Equal([0u, 4u], setting.UnlockedTone);
        Assert.Equal(4u, setting.ToneId);
        Assert.True(setting.GreenIsTojiru);
        Assert.True(setting.GreenIsAutoCostumeOn);
        Assert.Equal(3u, setting.GreenDispLevelChassis);
        Assert.Equal(2u, setting.GreenDispLevelSelf);

        var saveResult = await controller.SaveUserSetting("Red", 1, new UserSetting
        {
            MyDonName = "RED",
            Kigurumi = 7,
            UnlockedKigurumi = [0, 7],
            UnlockedTitle = [10],
            Title = "Red Title",
            TitlePlateId = 10,
            UnlockedTone = [0, 6],
            ToneId = 6,
            GreenIsTojiru = false,
            GreenIsAutoCostumeOn = false,
            GreenDispLevelChassis = 4,
            GreenDispLevelSelf = 3
        });

        Assert.IsType<NoContentResult>(saveResult);
        Assert.Equal("RED", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(7u, save.Costume1);
        Assert.Contains(7u, BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.Red.Limits.CostumeFlagBytes));
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.Red.Limits.TitleFlagBytes));
        Assert.Contains(6u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.Red.Limits.ToneFlagBytes));
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.True(save.IsChallengeCompe);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.NotNull(await fixture.Context.UserSaveDataBlue.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataGreen.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataYellow.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataNijiiro.FindAsync(1u));
    }

    [Fact]
    public async Task UserSettings_Red_RejectsInvalidSharedAc15Settings()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.DispLevelChassis = 2;
        fixture.Context.UserSaveDataRed.Add(save);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateUserSettingsController(fixture.Context);

        var result = await controller.SaveUserSetting("Red", 1, new UserSetting
        {
            MyDonName = "RED",
            GreenDispLevelChassis = 5
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(2u, save.DispLevelChassis);
        Assert.Equal("DON", (await fixture.Context.UserData.FindAsync(1u))!.MyDonName);
    }

    private static UserSettingsController CreateUserSettingsController(ITaikoDbContext context)
    {
        var authSettings = new AuthSettings { AuthenticationRequired = false };
        var httpContext = CreateHttpContext();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(Options.Create(authSettings))
            .BuildServiceProvider();

        return new UserSettingsController(context, Options.Create(authSettings))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(new AuthSettings { AuthenticationRequired = false }))
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }
}
