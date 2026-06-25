namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroProtocolLimitsTests
{
    [Fact]
    public void TryGet_Momoiro_ReturnsExplicitProfileWithUserDataOwnedCrowns()
    {
        var found = Ac15EraProfiles.TryGet(GameEra.Momoiro, out var profile);

        Assert.True(found, "Momoiro must have an explicit AC15 profile before route/userdata behavior depends on limits.");
        Assert.NotNull(profile);
        Assert.Equal(GameEra.Momoiro, profile.Era);
        Assert.Equal(Ac15CrownWirePlacement.UserData, profile.WirePlacement.CrownPlacement);
        Assert.False(profile.WirePlacement.HasInitialDataItemShopRows);
        Assert.False(profile.WirePlacement.HasInitialDataLegalTermsRows);
        Assert.False(profile.WirePlacement.HasTokkunTutorialFlagInUserData);
    }

    [Fact]
    public void TryGet_Momoiro_RecordsCatalogBackedAndAbsentFeatureFlags()
    {
        var found = Ac15EraProfiles.TryGet(GameEra.Momoiro, out var profile);

        Assert.True(found, "Momoiro feature flags must be explicit instead of inheriting a newer AC15 era wholesale.");
        Assert.NotNull(profile);
        Assert.True(profile.Features.NormalPlay);
        Assert.True(profile.Features.UserData);
        Assert.True(profile.Features.SelfBest);
        Assert.True(profile.Features.Crowns);
        Assert.True(profile.Features.Telops);
        Assert.True(profile.Features.Recommendations);
        Assert.False(profile.Features.InitialData);
        Assert.False(profile.Features.Folders);
        Assert.False(profile.Features.Taikojuku);
        Assert.False(profile.Features.Dani);
        Assert.False(profile.Features.ItemShop);
    }

    [Fact]
    public void TryGet_Momoiro_LabelsLowConfidenceFavoriteRecentAndCrownConstants()
    {
        var found = Ac15EraProfiles.TryGet(GameEra.Momoiro, out var profile);

        Assert.True(found, "Momoiro profile constants are RED contracts; favorite/crown values remain low-confidence until native proof improves.");
        Assert.NotNull(profile);
        Assert.Equal(128, profile.Limits.SongFlagBytes);
        Assert.Equal(5, profile.Limits.MaxFavoriteSongs);
        Assert.Equal(5, profile.Limits.MaxRecentSongs);
        Assert.Equal(380, profile.Limits.CrownSongCount);
        Assert.Equal(
            475,
            profile.Limits.CrownPackedBytes);
    }
}
