using GameDatabase.Entities;
using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface ISongBestDatumService
{
    public Task<List<SongBestDatum>> GetAllSongBestData(uint baid);

    public Task<SongBestDatum?> GetSongBestData(uint baid, uint songId, Difficulty difficulty);

    public Task UpdateSongBestData(SongBestDatum datum);

    public Task InsertSongBestData(SongBestDatum datum);

    public Task<List<SongBestData>> GetAllSongBestAsModel(uint baid);
}