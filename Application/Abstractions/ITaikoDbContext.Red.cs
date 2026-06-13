namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataRed> UserSaveDataRed { get; }
    DbSet<SongBestDatumRed> SongBestDataRed { get; }
    DbSet<SongPlayDatumRed> SongPlayDataRed { get; }
    DbSet<RedFavoriteSongs> RedFavoriteSongs { get; }
    DbSet<RedRecentSongs> RedRecentSongs { get; }
    DbSet<DanScoreDatumRed> DanScoreDataRed { get; }
    DbSet<DanStageScoreDatumRed> DanStageScoreDataRed { get; }
}
