using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongLeaderboardController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings) : BaseAdminController<SongLeaderboardController>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpGet("{songId}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<SongLeaderboardResponse>> GetSongLeaderboard(
        uint songId,
        [FromQuery] uint baid,
        [FromQuery] uint difficulty,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        // if baid is provided, check authentication
        if (baid != 0)
        {
            if (authSettings.AuthenticationRequired)
            {
                var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
                if (tokenInfo is null)
                {
                    return Unauthorized();
                }

                if (tokenInfo.Value.Baid != baid && !tokenInfo.Value.IsAdmin)
                {
                    return Forbid();
                }
            }
        }

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

        var leaderboard = new List<SongLeaderboard>();
        foreach (var score in scores)
        {
            var user = await context.UserData
                .Where(x => x.Baid == score.Baid)
                .FirstOrDefaultAsync();

            var rank = await context.SongBestData
                .Where(x => x.SongId == songId && x.Difficulty == diff && x.BestScore > score.BestScore)
                .CountAsync();

            leaderboard.Add(new SongLeaderboard
            {
                Rank = rank + 1,
                Baid = score.Baid,
                UserName = user?.MyDonName,
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
