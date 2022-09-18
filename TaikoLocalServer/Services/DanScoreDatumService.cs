namespace TaikoLocalServer.Services;

public class DanScoreDatumService : IDanScoreDatumService
{
    private readonly TaikoDbContext context;

    public DanScoreDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<List<DanScoreDatum>> GetDanScoreDatumByBaid(uint baid)
    {
        return await context.DanScoreData.Where(datum => datum.Baid == baid)
            .Include(datum => datum.DanStageScoreData)
            .ToListAsync();
    }

    public async Task<DanScoreDatum?> GetSingleDanScoreDatum(uint baid, uint danId)
    {
        return await context.DanScoreData.Include(datum => datum.DanStageScoreData)
            .FirstOrDefaultAsync(datum => datum.Baid == baid &&
                                          datum.DanId == danId);
    }

    public async Task InsertOrUpdateDanScoreDatum(DanScoreDatum datum)
    {
        var existing = await context.DanScoreData.FindAsync(datum.Baid, datum.DanId);
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