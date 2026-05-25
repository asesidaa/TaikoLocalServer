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

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
