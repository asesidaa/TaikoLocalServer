using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15EraProfileTests
{
    [Fact]
    public void BlueProfile_DeclaresSharedAc15ModulesAndBlueWireExtras()
    {
        var profile = Ac15EraProfiles.Blue;

        Assert.Equal(GameEra.Blue, profile.Era);
        Assert.True(profile.Features.NormalPlay);
        Assert.True(profile.Features.UserData);
        Assert.True(profile.Features.SelfBest);
        Assert.True(profile.Features.Crowns);
        Assert.True(profile.Features.InitialData);
        Assert.True(profile.Features.Folders);
        Assert.True(profile.Features.Telops);
        Assert.True(profile.Features.Taikojuku);
        Assert.True(profile.Features.Dani);
        Assert.True(profile.Features.ItemShop);
        Assert.True(profile.Features.Recommendations);

        Assert.Equal(128, profile.Limits.SongFlagBytes);
        Assert.Equal(1280, profile.Limits.CrownPackedBytes);
        Assert.Equal(5u, profile.Limits.MaxCourseLevel);
        Assert.Equal(5, profile.Limits.MaxFavoriteSongs);
        Assert.Equal(10, profile.Limits.MaxRecentSongs);
        Assert.Equal(Ac15CrownWirePlacement.DedicatedEndpoint, profile.WirePlacement.CrownPlacement);
        Assert.True(profile.WirePlacement.HasTokkunTutorialFlagInUserData);
        Assert.True(profile.WirePlacement.HasInitialDataLegalTermsRows);
    }

    [Fact]
    public void GreenProfile_DeclaresSharedAc15ModulesWithoutBlueExtras()
    {
        var profile = Ac15EraProfiles.Green;

        Assert.Equal(GameEra.Green, profile.Era);
        Assert.True(profile.Features.NormalPlay);
        Assert.True(profile.Features.UserData);
        Assert.True(profile.Features.SelfBest);
        Assert.True(profile.Features.Crowns);
        Assert.True(profile.Features.InitialData);
        Assert.True(profile.Features.Folders);
        Assert.True(profile.Features.Telops);
        Assert.True(profile.Features.Taikojuku);
        Assert.True(profile.Features.Dani);
        Assert.True(profile.Features.ItemShop);
        Assert.True(profile.Features.Recommendations);

        Assert.Equal(128, profile.Limits.SongFlagBytes);
        Assert.Equal(1280, profile.Limits.CrownPackedBytes);
        Assert.Equal(5u, profile.Limits.MaxCourseLevel);
        Assert.Equal(5, profile.Limits.MaxFavoriteSongs);
        Assert.Equal(10, profile.Limits.MaxRecentSongs);
        Assert.Equal(Ac15CrownWirePlacement.DedicatedEndpoint, profile.WirePlacement.CrownPlacement);
        Assert.False(profile.WirePlacement.HasTokkunTutorialFlagInUserData);
        Assert.False(profile.WirePlacement.HasInitialDataLegalTermsRows);
    }

    [Fact]
    public void YellowProfile_DeclaresCatalogAc15ModulesWithTokkunTutorialReadback()
    {
        var profile = Ac15EraProfiles.Yellow;

        Assert.Equal(GameEra.Yellow, profile.Era);
        Assert.True(profile.Features.InitialData);
        Assert.True(profile.Features.Folders);
        Assert.True(profile.Features.Telops);
        Assert.True(profile.Features.Taikojuku);
        Assert.True(profile.Features.Dani);
        Assert.True(profile.Features.ItemShop);
        Assert.True(profile.Features.Recommendations);
        Assert.True(profile.Features.NormalPlay);
        Assert.True(profile.Features.UserData);
        Assert.True(profile.Features.SelfBest);
        Assert.True(profile.Features.Crowns);

        Assert.Equal(128, profile.Limits.SongFlagBytes);
        Assert.Equal(1280, profile.Limits.CrownPackedBytes);
        Assert.Equal(5u, profile.Limits.MaxCourseLevel);
        Assert.Equal(5, profile.Limits.MaxFavoriteSongs);
        Assert.Equal(10, profile.Limits.MaxRecentSongs);
        Assert.Equal(Ac15CrownWirePlacement.DedicatedEndpoint, profile.WirePlacement.CrownPlacement);
        Assert.True(profile.WirePlacement.HasInitialDataItemShopRows);
        Assert.True(profile.WirePlacement.HasInitialDataLegalTermsRows);
        Assert.True(profile.WirePlacement.HasTokkunTutorialFlagInUserData);
    }

    [Fact]
    public async Task DefaultHooks_DoNotHandleSpecialModes()
    {
        var result = await Ac15EraProfiles.Blue.Hooks.TryHandleSpecialPlayModeAsync(
            new CommonPlayResultData(),
            new Ac15SpecialModeContext(1, GameEra.Blue),
            CancellationToken.None);

        Assert.Equal(Ac15SpecialModeAction.ContinueNormal, result.Action);
        Assert.Equal(1u, result.Result);
    }
}
