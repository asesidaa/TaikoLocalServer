using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IDanScoreDatumService
{
    public Task<List<DanScoreDatum>> GetDanScoreDataList(uint baid, DanType danType);

    public Task<DanScoreDatum?> GetSingleDanScoreDatum(uint baid, uint danId, DanType danType);

    public Task InsertOrUpdateDanScoreDatum(DanScoreDatum datum);

    public void TrackDanStageData(DanStageScoreDatum datum);
}