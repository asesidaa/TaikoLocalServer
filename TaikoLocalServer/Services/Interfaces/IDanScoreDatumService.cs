namespace TaikoLocalServer.Services.Interfaces;

public interface IDanScoreDatumService
{
    public Task<List<DanScoreDatum>> GetDanScoreDataList(uint baid, DanType danType);
}