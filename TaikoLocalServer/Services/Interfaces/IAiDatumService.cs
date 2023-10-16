using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IAiDatumService
{
    public Task<List<AiScoreDatum>> GetAllAiScoreById(ulong baid);

    public Task<AiScoreDatum?> GetSongAiScore(ulong baid, uint songId, Difficulty difficulty);

    public Task UpdateSongAiScore(AiScoreDatum datum);

    public Task InsertSongAiScore(AiScoreDatum datum);
}