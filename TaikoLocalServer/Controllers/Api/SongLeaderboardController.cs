using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Models.Responses;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]

public class SongLeaderboardController(ISongLeaderboardService songLeaderboardService,IAuthService authService, IOptions<AuthSettings> settings) 
    : BaseController<SongLeaderboardController>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpGet("{songId}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<SongLeaderboardResponse>> GetSongLeaderboard(uint songId, [FromQuery] uint baid, [FromQuery] uint difficulty, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        // if baid id is provided, check authentication
        if (!baid.Equals(0))
        {
            if (authSettings.AuthenticationRequired)
            {
                var tokenInfo = authService.ExtractTokenInfo(HttpContext);
                if (tokenInfo is null)
                {
                    return Unauthorized();
                }

                if (tokenInfo.Value.baid != baid && !tokenInfo.Value.isAdmin)
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

        var (leaderboard, userScore, totalPages, totalScores) = await songLeaderboardService.GetSongLeaderboard(songId, (Difficulty)difficulty, (int)baid, page, limit);

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