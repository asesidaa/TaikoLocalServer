namespace SharedProject.Models.Responses;

public class SongLeaderboardResponse
{
    public List<SongLeaderboard> LeaderboardData { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}