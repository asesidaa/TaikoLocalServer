namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataWhite> UserSaveDataWhite { get; }
    DbSet<SongBestDatumWhite> SongBestDataWhite { get; }
    DbSet<SongPlayDatumWhite> SongPlayDataWhite { get; }
    DbSet<WhiteFavoriteSongs> WhiteFavoriteSongs { get; }
    DbSet<WhiteRecentSongs> WhiteRecentSongs { get; }
    DbSet<WhiteTokkunStageResult> WhiteTokkunStageResults { get; }
    DbSet<DanScoreDatumWhite> DanScoreDataWhite { get; }
    DbSet<DanStageScoreDatumWhite> DanStageScoreDataWhite { get; }
    DbSet<WhiteDonChallengeRawFact> WhiteDonChallengeRawFacts { get; }
    DbSet<WhiteDonChallengeProgress> WhiteDonChallengeProgress { get; }
}
