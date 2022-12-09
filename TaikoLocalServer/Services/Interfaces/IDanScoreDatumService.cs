using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IDanScoreDatumService
{
    public Task<List<DanScoreDatum>> GetDanScoreDatumByBaid(uint baid);

    public Task<DanScoreDatum?> GetSingleDanScoreDatum(uint baid, uint danId);

    public Task InsertOrUpdateDanScoreDatum(DanScoreDatum datum);
}