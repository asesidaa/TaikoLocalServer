using SharedProject.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface ISongPlayDatumService
{
    public Task<List<SongPlayDatum>> GetSongPlayDatumByBaid(uint baid);

    public Task AddSongPlayDatum(SongPlayDatum datum);
}