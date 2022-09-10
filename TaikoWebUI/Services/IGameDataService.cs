using SharedProject.Enums;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Services;

public interface IGameDataService
{
    public Task InitializeAsync(string dataBaseUrl);
    
    public string GetMusicNameBySongId(uint songId);

    public string GetMusicArtistBySongId(uint songId);

    public SongGenre GetMusicGenreBySongId(uint songId);

    public int GetMusicIndexBySongId(uint songId);
}