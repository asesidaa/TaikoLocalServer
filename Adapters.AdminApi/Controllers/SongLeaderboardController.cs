namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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

        var songFilter = context.SongBestDataNijiiro
            .Where(x => x.SongId == songId && x.Difficulty == diff);

        var totalScores = await songFilter.CountAsync(HttpContext.RequestAborted);

        var totalPages = totalScores / limit;
        if (totalScores % limit > 0)
        {
            totalPages++;
        }

        var orderedScores = await songFilter
            .OrderByDescending(x => x.BestScore)
            .ThenByDescending(x => x.BestRate)
            .ThenByDescending(x => x.BestCrown)
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .Select(x => new
            {
                x.Baid,
                x.BestScore,
                x.BestRate,
                x.BestCrown,
                x.BestScoreRank
            })
            .ToListAsync(HttpContext.RequestAborted);

        // Batch-resolve usernames for the page in a single query.
        var pageBaids = orderedScores.Select(s => s.Baid).ToList();
        var userNames = await context.UserData
            .Where(u => pageBaids.Contains(u.Baid))
            .AsNoTracking()
            .ToDictionaryAsync(u => u.Baid, u => u.MyDonName, HttpContext.RequestAborted);

        var pageScoreSet = orderedScores.Select(s => s.BestScore).ToHashSet();
        var rankByScore = new Dictionary<uint, int>();
        if (pageScoreSet.Count > 0)
        {
            var minPageScore = pageScoreSet.Min();
            var scoreBuckets = await songFilter
                .Where(x => x.BestScore >= minPageScore)
                .GroupBy(x => x.BestScore)
                .Select(g => new { BestScore = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync(HttpContext.RequestAborted);

            var aboveCount = 0;
            foreach (var bucket in scoreBuckets.OrderByDescending(b => b.BestScore))
            {
                if (pageScoreSet.Contains(bucket.BestScore))
                {
                    rankByScore[bucket.BestScore] = aboveCount + 1;
                }

                aboveCount += bucket.Count;
            }
        }

        var leaderboard = orderedScores
            .Select((s, i) => new SongLeaderboard
            {
                Rank = rankByScore.GetValueOrDefault(s.BestScore, (page - 1) * limit + i + 1),
                Baid = s.Baid,
                UserName = userNames.GetValueOrDefault(s.Baid),
                BestScore = s.BestScore,
                BestRate = s.BestRate,
                BestCrown = s.BestCrown,
                BestScoreRank = s.BestScoreRank
            })
            .ToList();

        SongLeaderboard? userScore = null;
        if (baid != 0)
        {
            var score = await context.SongBestDataNijiiro
                .Where(x => x.SongId == songId && x.Difficulty == diff && x.Baid == baid)
                .AsNoTracking()
                .FirstOrDefaultAsync(HttpContext.RequestAborted);

            if (score != null)
            {
                var aboveCount = await songFilter
                    .CountAsync(x => x.BestScore > score.BestScore, HttpContext.RequestAborted);
                var user = await context.UserData
                    .Where(x => x.Baid == baid)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(HttpContext.RequestAborted);

                userScore = new SongLeaderboard
                {
                    Rank = aboveCount + 1,
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
