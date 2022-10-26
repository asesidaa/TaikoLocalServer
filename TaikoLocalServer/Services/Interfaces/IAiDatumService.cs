using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IAiDatumService
{
    public Task<List<AiScoreDatum>> GetAllAiScoreById(uint baid);

    public Task<AiScoreDatum?> GetSongAiScore(uint baid, uint songId, Difficulty difficulty);

    public Task UpdateSongAiScore(AiScoreDatum datum);

    public Task InsertSongAiScore(AiScoreDatum datum);
}