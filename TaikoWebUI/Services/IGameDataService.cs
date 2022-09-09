namespace TaikoWebUI.Services;

public interface IGameDataService
{
    public Task InitializeAsync(string dataBaseUrl);
    
    public string GetMusicNameBySongId(uint songId);

    public string GetMusicArtistBySongId(uint songId);
}