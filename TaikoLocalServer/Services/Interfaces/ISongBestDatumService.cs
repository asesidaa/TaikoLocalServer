using GameDatabase.Entities;
using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface ISongBestDatumService
{
    public Task<List<SongBestDatum>> GetAllSongBestData(ulong baid);

    public Task<SongBestDatum?> GetSongBestData(ulong baid, uint songId, Difficulty difficulty);

    public Task UpdateSongBestData(SongBestDatum datum);

    public Task InsertSongBestData(SongBestDatum datum);

    public Task<List<SongBestData>> GetAllSongBestAsModel(ulong baid);
}