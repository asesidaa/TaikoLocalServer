using GameDatabase.Context;

namespace TaikoLocalServer.Services;

class SongPlayDatumService : ISongPlayDatumService
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
}