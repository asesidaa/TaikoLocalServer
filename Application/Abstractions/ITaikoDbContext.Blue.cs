namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataBlue> UserSaveDataBlue { get; }
    DbSet<SongBestDatumBlue> SongBestDataBlue { get; }
    DbSet<SongPlayDatumBlue> SongPlayDataBlue { get; }
    DbSet<BlueFavoriteSongs> BlueFavoriteSongs { get; }
    DbSet<BlueRecentSongs> BlueRecentSongs { get; }
    DbSet<DanScoreDatumBlue> DanScoreDataBlue { get; }
    DbSet<DanStageScoreDatumBlue> DanStageScoreDataBlue { get; }
}
