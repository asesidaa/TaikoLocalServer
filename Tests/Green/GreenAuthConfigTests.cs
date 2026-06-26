using Microsoft.AspNetCore.Mvc;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenAuthConfigTests
{
    [Fact]
    public void GetConfig_ReturnsOnlyEnabledEras()
    {
        var controller = new AuthController(
            new ThrowingTaikoDbContext(),
            new ThrowingJwtTokenService(),
            Options.Create(new AuthSettings { AuthenticationRequired = false }),
            Options.Create(new ServerSettings
            {
                Eras = new Dictionary<string, EraSettings>
                {
                    [nameof(GameEra.Nijiiro)] = new() { Enabled = false },
                    [nameof(GameEra.Green)] = new() { Enabled = true }
                }
            }));

        var result = controller.GetConfig();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ClientAuthConfigResponse>(ok.Value);
        Assert.Equal(["Green"], response.EnabledEras);
    }

    [Fact]
    public void GetConfig_ReturnsFavoriteSongLimitsForEnabledLimitedEras()
    {
        var controller = new AuthController(
            new ThrowingTaikoDbContext(),
            new ThrowingJwtTokenService(),
            Options.Create(new AuthSettings { AuthenticationRequired = false }),
            Options.Create(new ServerSettings
            {
                Eras = new Dictionary<string, EraSettings>
                {
                    [nameof(GameEra.Nijiiro)] = new() { Enabled = true },
                    [nameof(GameEra.Green)] = new() { Enabled = true },
                    [nameof(GameEra.Blue)] = new() { Enabled = false },
                    [nameof(GameEra.Red)] = new() { Enabled = true },
                    [nameof(GameEra.White)] = new() { Enabled = true },
                    [nameof(GameEra.Kimidori)] = new() { Enabled = true }
                }
            }));

        var result = controller.GetConfig();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ClientAuthConfigResponse>(ok.Value);
        Assert.NotNull(response.FavoriteSongLimits);
        Assert.Equal(Ac15EraProfiles.Green.Limits.MaxFavoriteSongs, response.FavoriteSongLimits[nameof(GameEra.Green)]);
        Assert.Equal(Ac15EraProfiles.Red.Limits.MaxFavoriteSongs, response.FavoriteSongLimits[nameof(GameEra.Red)]);
        Assert.Equal(Ac15EraProfiles.White.Limits.MaxFavoriteSongs, response.FavoriteSongLimits[nameof(GameEra.White)]);
        Assert.Equal(Ac15EraProfiles.Kimidori.Limits.MaxFavoriteSongs, response.FavoriteSongLimits[nameof(GameEra.Kimidori)]);
        Assert.DoesNotContain(nameof(GameEra.Nijiiro), response.FavoriteSongLimits.Keys);
        Assert.DoesNotContain(nameof(GameEra.Blue), response.FavoriteSongLimits.Keys);
    }

    private sealed class ThrowingJwtTokenService : IJwtTokenService
    {
        public string IssueToken(uint baid, bool isAdmin)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingTaikoDbContext : ITaikoDbContext
    {
        public DbSet<UserDatum> UserData => throw new NotSupportedException();
        public DbSet<Card> Cards => throw new NotSupportedException();
        public DbSet<Credential> Credentials => throw new NotSupportedException();
        public DbSet<Token> Tokens => throw new NotSupportedException();
        public DbSet<UserSaveDataNijiiro> UserSaveDataNijiiro => throw new NotSupportedException();
        public DbSet<SongBestDatumNijiiro> SongBestDataNijiiro => throw new NotSupportedException();
        public DbSet<SongPlayDatumNijiiro> SongPlayDataNijiiro => throw new NotSupportedException();
        public DbSet<DanScoreDatumNijiiro> DanScoreDataNijiiro => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumNijiiro> DanStageScoreDataNijiiro => throw new NotSupportedException();
        public DbSet<AiScoreDatumNijiiro> AiScoreDataNijiiro => throw new NotSupportedException();
        public DbSet<AiSectionScoreDatumNijiiro> AiSectionScoreDataNijiiro => throw new NotSupportedException();
        public DbSet<UserSaveDataGreen> UserSaveDataGreen => throw new NotSupportedException();
        public DbSet<SongBestDatumGreen> SongBestDataGreen => throw new NotSupportedException();
        public DbSet<SongPlayDatumGreen> SongPlayDataGreen => throw new NotSupportedException();
        public DbSet<DanScoreDatumGreen> DanScoreDataGreen => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumGreen> DanStageScoreDataGreen => throw new NotSupportedException();
        public DbSet<GhostStageSectionDatumGreen> GhostStageSectionDataGreen => throw new NotSupportedException();
        public DbSet<GreenFavoriteSongs> GreenFavoriteSongs => throw new NotSupportedException();
        public DbSet<GreenRecentSongs> GreenRecentSongs => throw new NotSupportedException();
        public DbSet<GreenFriends> GreenFriends => throw new NotSupportedException();
        public DbSet<GreenGhostTokens> GreenGhostTokens => throw new NotSupportedException();
        public DbSet<GreenGhostWinnings> GreenGhostWinnings => throw new NotSupportedException();
        public DbSet<GreenShopSeasonState> GreenShopSeasonStates => throw new NotSupportedException();
        public DbSet<GreenShopItemState> GreenShopItemStates => throw new NotSupportedException();
        public DbSet<UserSaveDataBlue> UserSaveDataBlue => throw new NotSupportedException();
        public DbSet<SongBestDatumBlue> SongBestDataBlue => throw new NotSupportedException();
        public DbSet<SongPlayDatumBlue> SongPlayDataBlue => throw new NotSupportedException();
        public DbSet<BlueFavoriteSongs> BlueFavoriteSongs => throw new NotSupportedException();
        public DbSet<BlueRecentSongs> BlueRecentSongs => throw new NotSupportedException();
        public DbSet<DanScoreDatumBlue> DanScoreDataBlue => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumBlue> DanStageScoreDataBlue => throw new NotSupportedException();
        public DbSet<BlueShopSeasonState> BlueShopSeasonStates => throw new NotSupportedException();
        public DbSet<BlueShopItemState> BlueShopItemStates => throw new NotSupportedException();
        public DbSet<BlueBattleUserState> BlueBattleUserStates => throw new NotSupportedException();
        public DbSet<BlueBattleNpcState> BlueBattleNpcStates => throw new NotSupportedException();
        public DbSet<BlueBattleTokenState> BlueBattleTokenStates => throw new NotSupportedException();
        public DbSet<BlueBattleStageResult> BlueBattleStageResults => throw new NotSupportedException();
        public DbSet<BlueTokkunStageResult> BlueTokkunStageResults => throw new NotSupportedException();
        public DbSet<UserSaveDataYellow> UserSaveDataYellow => throw new NotSupportedException();
        public DbSet<SongBestDatumYellow> SongBestDataYellow => throw new NotSupportedException();
        public DbSet<SongPlayDatumYellow> SongPlayDataYellow => throw new NotSupportedException();
        public DbSet<YellowFavoriteSongs> YellowFavoriteSongs => throw new NotSupportedException();
        public DbSet<YellowRecentSongs> YellowRecentSongs => throw new NotSupportedException();
        public DbSet<DanScoreDatumYellow> DanScoreDataYellow => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumYellow> DanStageScoreDataYellow => throw new NotSupportedException();
        public DbSet<YellowShopSeasonState> YellowShopSeasonStates => throw new NotSupportedException();
        public DbSet<YellowShopItemState> YellowShopItemStates => throw new NotSupportedException();
        public DbSet<YellowTokkunStageResult> YellowTokkunStageResults => throw new NotSupportedException();
        public DbSet<UserSaveDataRed> UserSaveDataRed => throw new NotSupportedException();
        public DbSet<SongBestDatumRed> SongBestDataRed => throw new NotSupportedException();
        public DbSet<SongPlayDatumRed> SongPlayDataRed => throw new NotSupportedException();
        public DbSet<RedFavoriteSongs> RedFavoriteSongs => throw new NotSupportedException();
        public DbSet<RedRecentSongs> RedRecentSongs => throw new NotSupportedException();
        public DbSet<DanScoreDatumRed> DanScoreDataRed => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumRed> DanStageScoreDataRed => throw new NotSupportedException();
        public DbSet<RedDonChallengeRawFact> RedDonChallengeRawFacts => throw new NotSupportedException();
        public DbSet<RedDonChallengeProgress> RedDonChallengeProgress => throw new NotSupportedException();
        public DbSet<UserSaveDataWhite> UserSaveDataWhite => throw new NotSupportedException();
        public DbSet<SongBestDatumWhite> SongBestDataWhite => throw new NotSupportedException();
        public DbSet<SongPlayDatumWhite> SongPlayDataWhite => throw new NotSupportedException();
        public DbSet<WhiteFavoriteSongs> WhiteFavoriteSongs => throw new NotSupportedException();
        public DbSet<WhiteRecentSongs> WhiteRecentSongs => throw new NotSupportedException();
        public DbSet<WhiteTokkunStageResult> WhiteTokkunStageResults => throw new NotSupportedException();
        public DbSet<DanScoreDatumWhite> DanScoreDataWhite => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumWhite> DanStageScoreDataWhite => throw new NotSupportedException();
        public DbSet<WhiteDonChallengeRawFact> WhiteDonChallengeRawFacts => throw new NotSupportedException();
        public DbSet<WhiteDonChallengeProgress> WhiteDonChallengeProgress => throw new NotSupportedException();
        public DbSet<UserSaveDataMurasaki> UserSaveDataMurasaki => throw new NotSupportedException();
        public DbSet<SongBestDatumMurasaki> SongBestDataMurasaki => throw new NotSupportedException();
        public DbSet<SongPlayDatumMurasaki> SongPlayDataMurasaki => throw new NotSupportedException();
        public DbSet<MurasakiFavoriteSongs> MurasakiFavoriteSongs => throw new NotSupportedException();
        public DbSet<MurasakiRecentSongs> MurasakiRecentSongs => throw new NotSupportedException();
        public DbSet<DanScoreDatumMurasaki> DanScoreDataMurasaki => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumMurasaki> DanStageScoreDataMurasaki => throw new NotSupportedException();
        public DbSet<UserSaveDataKimidori> UserSaveDataKimidori => throw new NotSupportedException();
        public DbSet<SongBestDatumKimidori> SongBestDataKimidori => throw new NotSupportedException();
        public DbSet<SongPlayDatumKimidori> SongPlayDataKimidori => throw new NotSupportedException();
        public DbSet<KimidoriFavoriteSongs> KimidoriFavoriteSongs => throw new NotSupportedException();
        public DbSet<KimidoriRecentSongs> KimidoriRecentSongs => throw new NotSupportedException();
        public DbSet<DanScoreDatumKimidori> DanScoreDataKimidori => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumKimidori> DanStageScoreDataKimidori => throw new NotSupportedException();
        public DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro => throw new NotSupportedException();
        public DbSet<SongBestDatumMomoiro> SongBestDataMomoiro => throw new NotSupportedException();
        public DbSet<SongPlayDatumMomoiro> SongPlayDataMomoiro => throw new NotSupportedException();
        public DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs => throw new NotSupportedException();
        public DbSet<MomoiroRecentSongs> MomoiroRecentSongs => throw new NotSupportedException();
        public DbSet<DanScoreDatumMomoiro> DanScoreDataMomoiro => throw new NotSupportedException();
        public DbSet<DanStageScoreDatumMomoiro> DanStageScoreDataMomoiro => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
