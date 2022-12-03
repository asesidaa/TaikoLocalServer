using SharedProject.Models;
using SharedProject.Models.Responses;
using Swan.Mapping;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class DanBestDataController : BaseController<DanBestDataController>
{
    private readonly IDanScoreDatumService danScoreDatumService;

    public DanBestDataController(IDanScoreDatumService danScoreDatumService)
    {
        this.danScoreDatumService = danScoreDatumService;
    }

    [HttpGet("{baid}")]
    public async Task<IActionResult> GetDanBestData(uint baid)
    {
        var danScores = await danScoreDatumService.GetDanScoreDatumByBaid(baid);
        var danDataList = new List<DanBestData>();

        foreach (var danScore in danScores)
        {
            var danData = danScore.CopyPropertiesToNew<DanBestData>();
            danData.DanBestStageDataList = danScore.DanStageScoreData
                .Select(datum => datum.CopyPropertiesToNew<DanBestStageData>()).ToList();
            danDataList.Add(danData);
        }

        return Ok(new DanBestDataResponse
        {
            DanBestDataList = danDataList
        });
    }
}