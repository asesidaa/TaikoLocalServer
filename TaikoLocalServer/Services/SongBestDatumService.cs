using TaikoLocalServer.Services.Interfaces;

namespace TaikoLocalServer.Services;

public class SongBestDatumService : ISongBestDatumService
{
    private readonly TaikoDbContext context;

    public SongBestDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<List<SongBestDatum>> GetAllSongBestData(uint baid)
    {
        return await context.SongBestData.Where(datum => datum.Baid == baid).ToListAsync();
    }

    public async Task UpdateOrInsertSongBestDatum(SongBestDatum datum)
    {
        var existing = await context.SongBestData.AnyAsync(
            bestDatum => bestDatum.Baid == datum.Baid &&
                         bestDatum.Difficulty == datum.Difficulty &&
                         bestDatum.SongId == datum.SongId);
        if (existing)
        {
            context.SongBestData.Update(datum);
            await context.SaveChangesAsync();
            return;
        }

        await context.SongBestData.AddAsync(datum);
        await context.SaveChangesAsync();
    }
}