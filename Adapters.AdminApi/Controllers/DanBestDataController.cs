using Swan.Mapping;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DanBestDataController(ITaikoDbContext context) : BaseAdminController<DanBestDataController>
{
    [HttpGet("{baid}")]
    public async Task<IActionResult> GetDanBestData(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

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
