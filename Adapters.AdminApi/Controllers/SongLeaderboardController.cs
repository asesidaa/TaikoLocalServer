namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class SongLeaderboardController(ITaikoDbContext context) : BaseAdminController<SongLeaderboardController>
{
    private readonly ITaikoDbContext context = context;

    [HttpGet("{songId}")]
    public Task<ActionResult<SongLeaderboardResponse>> GetSongLeaderboard(
        uint songId,
        [FromQuery] uint baid,
        [FromQuery] uint difficulty,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
        => GetSongLeaderboard(nameof(GameEra.Nijiiro), songId, baid, difficulty, page, limit);

    [HttpGet("/api/{era}/[controller]/{songId}")]
    public async Task<ActionResult<SongLeaderboardResponse>> GetSongLeaderboard(
        string era,
        uint songId,
        [FromQuery] uint baid,
        [FromQuery] uint difficulty,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

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

        var rows = gameEra switch
        {
            GameEra.Nijiiro => await GetNijiiroLeaderboardRows(songId, diff),
            GameEra.Green => await GetGreenLeaderboardRows(songId, diff),
            GameEra.Blue => await GetBlueLeaderboardRows(songId, diff),
            GameEra.Yellow => await GetYellowLeaderboardRows(songId, diff),
            GameEra.Red => await GetRedLeaderboardRows(songId, diff),
            GameEra.White => await GetWhiteLeaderboardRows(songId, diff),
            GameEra.Murasaki => await GetMurasakiLeaderboardRows(songId, diff),
            _ => null
        };

        if (rows is null)
        {
            return EraRoute.BadEra(era);
        }

        var totalScores = rows.Count;

        var totalPages = totalScores / limit;
        if (totalScores % limit > 0)
        {
            totalPages++;
        }

        var orderedScores = rows
            .OrderByDescending(x => x.BestScore)
            .ThenByDescending(x => x.BestRate)
            .ThenByDescending(x => x.BestCrown)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

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
            var scoreBuckets = rows
                .Where(x => x.BestScore >= minPageScore)
                .GroupBy(x => x.BestScore)
                .Select(g => new { BestScore = g.Key, Count = g.Count() })
                .ToList();

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
            var score = rows.FirstOrDefault(x => x.Baid == baid);

            if (score != null)
            {
                var aboveCount = rows.Count(x => x.BestScore > score.BestScore);
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

    private async Task<List<LeaderboardScoreRow>> GetNijiiroLeaderboardRows(uint songId, Difficulty difficulty)
    {
        return await context.SongBestDataNijiiro
            .Where(x => x.SongId == songId && x.Difficulty == difficulty)
            .AsNoTracking()
            .Select(x => new LeaderboardScoreRow
            {
                Baid = x.Baid,
                BestScore = x.BestScore,
                BestRate = x.BestRate,
                BestCrown = x.BestCrown,
                BestScoreRank = x.BestScoreRank
            })
            .ToListAsync(HttpContext.RequestAborted);
    }

    private async Task<List<LeaderboardScoreRow>> GetGreenLeaderboardRows(uint songId, Difficulty difficulty)
    {
        return await context.SongBestDataGreen
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

    private async Task<List<LeaderboardScoreRow>> GetBlueLeaderboardRows(uint songId, Difficulty difficulty)
    {
        return await context.SongBestDataBlue
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

    private sealed class LeaderboardScoreRow
    {
        public uint Baid { get; init; }

        public uint BestScore { get; init; }

        public uint BestRate { get; init; }

        public CrownType BestCrown { get; init; }

        public ScoreRank BestScoreRank { get; init; }
    }
}
