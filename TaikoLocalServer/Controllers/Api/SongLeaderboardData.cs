using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Models.Responses;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]

public class SongLeaderboardData(ISongLeaderboardService songLeaderboardService,IAuthService authService, IOptions<AuthSettings> settings) 
    : BaseController<SongLeaderboardData>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpGet("{baid}/{songId}/{difficulty}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]

    public async Task<ActionResult<SongLeaderboardResponse>> GetSongLeaderboard(uint baid, uint songId, uint difficulty)
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

        var leaderboard = await songLeaderboardService.GetSongLeaderboard(songId, (Difficulty)difficulty, (int)baid);
        
        return Ok(new SongLeaderboardResponse
        {
            Leaderboard = leaderboard
        });
    }
}