using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Models.Responses;
using Swan.Mapping;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class DanBestDataController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings) : BaseController<DanBestDataController>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> GetDanBestData(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.IsAdmin && tokenInfo.Value.Baid != baid)
            {
                return Forbid();
            }
        }

        // FIXME: Handle gaiden in here and web ui
        var danScores = await context.DanScoreData
            .Where(d => d.Baid == baid && d.DanType == DanType.Normal)
            .Include(d => d.DanStageScoreData)
            .ToListAsync();

        var danDataList = new List<DanBestData>();
        foreach (var danScore in danScores)
        {
            var danData = danScore.CopyPropertiesToNew<DanBestData>();
            danData.DanBestStageDataList = danScore.DanStageScoreData
                .Select(datum => datum.CopyPropertiesToNew<DanBestStageData>())
                .ToList();
            danDataList.Add(danData);
        }

        return Ok(new DanBestDataResponse
        {
            DanBestDataList = danDataList
        });
    }
}
