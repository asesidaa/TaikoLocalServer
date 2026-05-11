namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<SongBestDatum> SongBestData { get; }
    DbSet<SongPlayDatum> SongPlayData { get; }
    DbSet<DanScoreDatum> DanScoreData { get; }
    DbSet<DanStageScoreDatum> DanStageScoreData { get; }
    DbSet<AiScoreDatum> AiScoreData { get; }
    DbSet<AiSectionScoreDatum> AiSectionScoreData { get; }
}
