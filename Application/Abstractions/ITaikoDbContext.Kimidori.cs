namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserSaveDataKimidori> UserSaveDataKimidori { get; }
    DbSet<SongBestDatumKimidori> SongBestDataKimidori { get; }
    DbSet<SongPlayDatumKimidori> SongPlayDataKimidori { get; }
    DbSet<KimidoriFavoriteSongs> KimidoriFavoriteSongs { get; }
    DbSet<KimidoriRecentSongs> KimidoriRecentSongs { get; }
    DbSet<DanScoreDatumKimidori> DanScoreDataKimidori { get; }
    DbSet<DanStageScoreDatumKimidori> DanStageScoreDataKimidori { get; }
}
