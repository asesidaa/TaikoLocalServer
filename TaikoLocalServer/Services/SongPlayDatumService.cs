using GameDatabase.Context;
using GameDatabase.Entities;

namespace TaikoLocalServer.Services;

internal class SongPlayDatumService : ISongPlayDatumService
{
    private readonly TaikoDbContext context;

    public SongPlayDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<List<SongPlayDatum>> GetSongPlayDatumByBaid(uint baid)
    {
        return await context.SongPlayData.Where(datum => datum.Baid == baid).ToListAsync();
    }

    public async Task AddSongPlayDatum(SongPlayDatum datum)
    {
        context.SongPlayData.Add(datum);
        await context.SaveChangesAsync();
    }
}