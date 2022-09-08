namespace TaikoLocalServer.Services.Interfaces;

public interface ISongBestDatumService
{
    public Task<List<SongBestDatum>> GetAllSongBestData(uint baid);

    public Task UpdateOrInsertSongBestDatum(SongBestDatum datum);
}