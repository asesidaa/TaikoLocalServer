using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface ISongBestDatumService
{
    public Task<List<SongBestDatum>> GetAllSongBestData(uint baid);

    public Task<List<SongBestData>> GetAllSongBestAsModel(uint baid);
}