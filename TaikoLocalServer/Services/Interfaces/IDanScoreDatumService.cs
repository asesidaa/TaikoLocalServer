using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IDanScoreDatumService
{
    public Task<List<DanScoreDatum>> GetDanScoreDatumByBaid(ulong baid);
    
    public Task<DanScoreDatum?> GetSingleDanScoreDatum(ulong baid, uint danId);

    public Task InsertOrUpdateDanScoreDatum(DanScoreDatum datum);
}