namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class SongLeaderboardController
{
    private async Task<List<LeaderboardScoreRow>> GetMomoiroLeaderboardRows(uint songId, Difficulty difficulty)
    {
        return await context.SongBestDataMomoiro
            .Where(x => x.SongId == songId && x.Difficulty == difficulty && !x.IsShin)
            .AsNoTracking()
            .Select(x => new LeaderboardScoreRow
            {
                Baid = x.Baid,
                BestScore = x.BestScore,
                BestRate = x.BestRate,
                BestCrown = x.BestCrown,
                BestScoreRank = ScoreRank.None
            })
            .ToListAsync(HttpContext.RequestAborted);
    }
}
