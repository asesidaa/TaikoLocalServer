using System.Collections.Immutable;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Services;

public interface IGameDataService
{
    public Task InitializeAsync(string dataBaseUrl);

    public string GetMusicNameBySongId(uint songId);

    public string GetMusicArtistBySongId(uint songId);

    public SongGenre GetMusicGenreBySongId(uint songId);

    public int GetMusicIndexBySongId(uint songId);

    public DanData GetDanDataById(uint danId);

    public int GetMusicStarLevel(uint songId, Difficulty difficulty);

    public string GetHeadTitle(uint index);
    public string GetKigurumiTitle(uint index);
    public string GetBodyTitle(uint index);
    public string GetFaceTitle(uint index);
    public string GetPuchiTitle(uint index);

    public ImmutableHashSet<Title> GetTitles();
}