namespace TaikoLocalServer.Application.Abstractions;

public interface ITaikoDbContext
{
    DbSet<UserDatum> UserData { get; }
    DbSet<Card> Cards { get; }
    DbSet<Credential> Credentials { get; }
    DbSet<Token> Tokens { get; }
    DbSet<SongBestDatum> SongBestData { get; }
    DbSet<SongPlayDatum> SongPlayData { get; }
    DbSet<DanScoreDatum> DanScoreData { get; }
    DbSet<DanStageScoreDatum> DanStageScoreData { get; }
    DbSet<AiScoreDatum> AiScoreData { get; }
    DbSet<AiSectionScoreDatum> AiSectionScoreData { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
