using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Models.Responses;
using Swan.Mapping;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class DanBestDataController(IDanScoreDatumService danScoreDatumService, IAuthService authService,
    IOptions<AuthSettings> settings) : BaseController<DanBestDataController>
{
    private readonly AuthSettings authSettings = settings.Value;
    
    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> GetDanBestData(uint baid)
    {
        if (authSettings.LoginRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.isAdmin && tokenInfo.Value.baid != baid)
            {
                return Forbid();
            }
        }
        
        // FIXME: Handle gaiden in here and web ui
        var danScores = await danScoreDatumService.GetDanScoreDataList(baid, DanType.Normal);
        var danDataList = new List<DanBestData>();

        foreach (var danScore in danScores)
        {
            var danData = danScore.CopyPropertiesToNew<DanBestData>();
            danData.DanBestStageDataList = danScore.DanStageScoreData.Select(datum => datum.CopyPropertiesToNew<DanBestStageData>()).ToList();
            danDataList.Add(danData);
        }

        return Ok(new DanBestDataResponse
        {
            DanBestDataList = danDataList
        });
    }
}