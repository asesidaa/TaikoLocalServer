namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataMurasaki> UserSaveDataMurasaki { get; }
    DbSet<SongBestDatumMurasaki> SongBestDataMurasaki { get; }
    DbSet<SongPlayDatumMurasaki> SongPlayDataMurasaki { get; }
    DbSet<MurasakiFavoriteSongs> MurasakiFavoriteSongs { get; }
    DbSet<MurasakiRecentSongs> MurasakiRecentSongs { get; }
    DbSet<DanScoreDatumMurasaki> DanScoreDataMurasaki { get; }
    DbSet<DanStageScoreDatumMurasaki> DanStageScoreDataMurasaki { get; }
}
