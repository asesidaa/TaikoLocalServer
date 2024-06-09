namespace SharedProject.Models.Responses;

public class SongLeaderboardResponse
{
    public List<SongLeaderboard> LeaderboardData { get; set; } = new();
    public SongLeaderboard? UserScore { get; set; } = null;
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; } = 0;

    public int TotalScores { get; set; } = 0;
}