using GameDatabase.Entities;
using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface ISongBestDatumService
{
    public Task<List<SongBestDatum>> GetAllSongBestData(ulong baid);

    public Task UpdateOrInsertSongBestDatum(SongBestDatum datum);

    public Task<List<SongBestData>> GetAllSongBestAsModel(ulong baid);
}