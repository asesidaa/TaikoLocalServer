namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<SongBestDatumNijiiro> SongBestDataNijiiro { get; }
    DbSet<SongPlayDatumNijiiro> SongPlayDataNijiiro { get; }
    DbSet<DanScoreDatumNijiiro> DanScoreDataNijiiro { get; }
    DbSet<DanStageScoreDatumNijiiro> DanStageScoreDataNijiiro { get; }
    DbSet<AiScoreDatumNijiiro> AiScoreDataNijiiro { get; }
    DbSet<AiSectionScoreDatumNijiiro> AiSectionScoreDataNijiiro { get; }
    DbSet<UserSaveDataNijiiro> UserSaveDataNijiiro { get; }
}
