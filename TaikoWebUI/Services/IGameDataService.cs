namespace TaikoWebUI.Services;

public interface IGameDataService
{
    public Task InitializeAsync(string dataBaseUrl);
    
    public string GetMusicNameBySongId(uint songId);

    public string GetMusicArtistBySongId(uint songId);

    public string GetCostumeTitleById(uint costumeId, string type);

    public SongGenre GetMusicGenreBySongId(uint songId);

    public int GetMusicIndexBySongId(uint songId);

    public DanData GetDanDataById(uint danId);

    public int GetMusicStarLevel(uint songId, Difficulty difficulty);
}