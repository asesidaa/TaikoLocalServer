namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class SongLeaderboardController(ITaikoDbContext context) : BaseAdminController<SongLeaderboardController>
{
    [HttpGet("{songId}")]
    public async Task<ActionResult<SongLeaderboardResponse>> GetSongLeaderboard(
        uint songId,
        [FromQuery] uint baid,
        [FromQuery] uint difficulty,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        if (baid != 0 && this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        if (page < 1)
        {
            return BadRequest(new { Message = "Page number cannot be less than 1." });
        }

        if (limit > 200)
        {
            return BadRequest(new { Message = "Limit cannot be greater than 200." });
        }

        if (difficulty < 1 || difficulty > 5)
        {
            return BadRequest(new { Message = "Invalid difficulty. Please provide a number between 1-5." });
        }

        var diff = (Difficulty)difficulty;

        var totalScores = await context.SongBestData
            .Where(x => x.SongId == songId && x.Difficulty == diff)
            .CountAsync();

        var totalPages = totalScores / limit;
        if (totalScores % limit > 0)
        {
            totalPages++;
        }

        var scores = await context.SongBestData
            .Where(x => x.SongId == songId && x.Difficulty == diff)
            .OrderByDescending(x => x.BestScore)
            .ThenByDescending(x => x.BestRate)
            .ThenByDescending(x => x.BestCrown)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        // Two queries for the page, not two per row: names in one go, and ranks from the first row's (ties share a rank).
        var pageBaids = scores.Select(s => s.Baid).ToList();
        var names = await context.UserData.Where(u => pageBaids.Contains(u.Baid))
            .ToDictionaryAsync(u => u.Baid, u => u.MyDonName, HttpContext.RequestAborted);
        var firstRank = scores.Count == 0 ? 0 : 1 + await context.SongBestData
            .CountAsync(x => x.SongId == songId && x.Difficulty == diff && x.BestScore > scores[0].BestScore, HttpContext.RequestAborted);

        var leaderboard = new List<SongLeaderboard>();
        for (var i = 0; i < scores.Count; i++)
        {
            var score = scores[i];
            // Below a lower score, everyone earlier (this page and the ones before) scored higher.
            var rank = i == 0 ? firstRank
                : score.BestScore != scores[i - 1].BestScore ? (page - 1) * limit + i + 1
                : leaderboard[i - 1].Rank;
            leaderboard.Add(new SongLeaderboard
            {
                Rank = rank,
                Baid = score.Baid,
                UserName = names.GetValueOrDefault(score.Baid),
                BestScore = score.BestScore,
                BestRate = score.BestRate,
                BestCrown = score.BestCrown,
                BestScoreRank = score.BestScoreRank
            });
        }

        SongLeaderboard? userScore = null;
        if (baid != 0)
        {
            var score = await context.SongBestData
                .Where(x => x.SongId == songId && x.Difficulty == diff && x.Baid == baid)
                .FirstOrDefaultAsync();

            if (score != null)
            {
                var user = await context.UserData
                    .Where(x => x.Baid == baid)
                    .FirstOrDefaultAsync();

                var rank = await context.SongBestData
                    .Where(x => x.SongId == songId && x.Difficulty == diff && x.BestScore > score.BestScore)
                    .CountAsync();

                userScore = new SongLeaderboard
                {
                    Rank = rank + 1,
                    Baid = score.Baid,
                    UserName = user?.MyDonName,
                    BestScore = score.BestScore,
                    BestRate = score.BestRate,
                    BestCrown = score.BestCrown,
                    BestScoreRank = score.BestScoreRank
                };
            }
        }

        return Ok(new SongLeaderboardResponse
        {
            LeaderboardData = leaderboard,
            UserScore = userScore,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalScores = totalScores
        });
    }
}
