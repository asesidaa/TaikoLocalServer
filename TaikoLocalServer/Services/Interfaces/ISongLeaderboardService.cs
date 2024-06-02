namespace TaikoLocalServer.Services.Interfaces;

using SharedProject.Models;
public interface ISongLeaderboardService
{
    public Task<List<SongLeaderboard>> GetSongLeaderboard(uint songId, Difficulty difficulty, int baid, int limit = 10);
}