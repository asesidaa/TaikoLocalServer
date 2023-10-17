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

    public async Task<List<DanScoreDatum>> GetDanScoreDataList(ulong baid, DanType danType)
    {
        return await context.DanScoreData.Where(datum => datum.Baid == baid && datum.DanType == danType)
            .Include(datum => datum.DanStageScoreData)
            .ToListAsync();
    }

    public async Task<DanScoreDatum?> GetSingleDanScoreDatum(ulong baid, uint danId, DanType danType)
    {
        return await context.DanScoreData.Include(datum => datum.DanStageScoreData)
            .FirstOrDefaultAsync(datum => datum.Baid == baid &&
                                          datum.DanId == danId &&
                                          datum.DanType == danType);
    }

    public async Task InsertOrUpdateDanScoreDatum(DanScoreDatum datum)
    {
        var existing = await context.DanScoreData.FindAsync(datum.Baid, datum.DanId, datum.DanType);
        if (existing is null)
        {
            context.DanScoreData.Add(datum);
            await context.SaveChangesAsync();
            return;
        }

        context.DanScoreData.Update(datum);
        await context.SaveChangesAsync();
    }
}