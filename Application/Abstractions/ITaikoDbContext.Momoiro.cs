namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro { get; }
    DbSet<SongBestDatumMomoiro> SongBestDataMomoiro { get; }
    DbSet<SongPlayDatumMomoiro> SongPlayDataMomoiro { get; }
    DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs { get; }
    DbSet<MomoiroRecentSongs> MomoiroRecentSongs { get; }
    DbSet<DanScoreDatumMomoiro> DanScoreDataMomoiro { get; }
    DbSet<DanStageScoreDatumMomoiro> DanStageScoreDataMomoiro { get; }
}
