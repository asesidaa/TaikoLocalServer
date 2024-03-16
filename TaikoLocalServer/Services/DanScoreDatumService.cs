using GameDatabase.Context;
using GameDatabase.Entities;

namespace TaikoLocalServer.Services;

public class DanScoreDatumService : IDanScoreDatumService
{
    private readonly TaikoDbContext context;

    public DanScoreDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<List<DanScoreDatum>> GetDanScoreDataList(uint baid, DanType danType)
    {
        return await context.DanScoreData.Where(datum => datum.Baid == baid && datum.DanType == danType)
            .Include(datum => datum.DanStageScoreData)
            .ToListAsync();
    }
}