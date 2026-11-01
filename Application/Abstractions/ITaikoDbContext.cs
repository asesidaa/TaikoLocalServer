namespace TaikoLocalServer.Application.Abstractions;

public interface ITaikoDbContext
{
    DbSet<UserDatum> UserData { get; }
    DbSet<Card> Cards { get; }
    DbSet<Credential> Credentials { get; }
    DbSet<UserWebProfile> UserWebProfiles { get; }
    DbSet<UserAvatar> UserAvatars { get; }
    DbSet<AccessRules> AccessRules { get; }
    DbSet<ServerSecrets> ServerSecrets { get; }
    DbSet<ActivityEntry> Activity { get; }
    DbSet<SignInAttempt> SignInAttempts { get; }
    DbSet<BannedCard> BannedCards { get; }
    DbSet<WebSession> WebSessions { get; }
    DbSet<ApiToken> ApiTokens { get; }
    DbSet<DashboardNotice> DashboardNotices { get; }
    DbSet<ServerStatus> ServerStatuses { get; }
    DbSet<TrustedBrowser> TrustedBrowsers { get; }
    DbSet<ServerOptions> ServerOptions { get; }
    DbSet<Cabinet> Cabinets { get; }
    DbSet<Token> Tokens { get; }
    DbSet<SongBestDatum> SongBestData { get; }
    DbSet<SongPlayDatum> SongPlayData { get; }
    DbSet<DanScoreDatum> DanScoreData { get; }
    DbSet<DanStageScoreDatum> DanStageScoreData { get; }
    DbSet<AiScoreDatum> AiScoreData { get; }
    DbSet<AiSectionScoreDatum> AiSectionScoreData { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
