namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserWebProfile> UserWebProfiles { get; }
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
    DbSet<Cabinet> Cabinets { get; }
    DbSet<MovieLabel> MovieLabels { get; }
    DbSet<ShopLiveSeason> ShopLiveSeasons { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Nijiiro: questions about a player's plays in order, which the database answers in one pass (window functions) instead of
    // every play being read: a heavy player has 100,000 of them. Skipped plays don't count, as in Play History.

    /// <summary>
    /// How many of <paramref name="baid"/>'s plays (of <paramref name="difficulty"/>, or all) beat every earlier play of
    /// their chart, and when the latest was. Earlier = by time, then save order.
    /// </summary>
    Task<(int Count, DateTime? Latest)> NewBestTotalsAsync(uint baid, Difficulty? difficulty, CancellationToken cancellationToken);

    /// <summary>
    /// Credits per player (one player, or all): a credit starts at song 0, where the song number goes back (its first song
    /// wasn't saved), or after <paramref name="sessionGap"/> without a play, as Play History counts them.
    /// </summary>
    Task<Dictionary<uint, int>> CreditCountsAsync(uint? baid, TimeSpan sessionGap, CancellationToken cancellationToken);
}
