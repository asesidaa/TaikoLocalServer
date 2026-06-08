using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/crownsdata.php")]
public class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Yellow CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataYellow
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var yellow = gameDataService.Yellow();
        var inflated = CrownsDataMappers.BuildRawInflatedBody(bestRows, yellow);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = yellow.SongHashVersion,
            HashCrownFlg = inflated
        });
    }
}
