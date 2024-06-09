namespace TaikoLocalServer.Services.Interfaces;

using SharedProject.Models;
public interface ISongLeaderboardService
{
    Task<(List<SongLeaderboard>, SongLeaderboard?, int, int)> GetSongLeaderboard(uint songId, Difficulty difficulty, int baid = 0, int page = 1, int limit = 10);
}