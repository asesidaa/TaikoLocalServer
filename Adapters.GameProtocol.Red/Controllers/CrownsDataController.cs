using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/crownsdata.php")]
[Route("/v08r01/chassis/crownsdata.php")]
public class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Red CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataRed
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var red = gameDataService.Red();
        var inflated = CrownsDataMappers.BuildRawInflatedBody(bestRows, red);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = red.SongHashVersion,
            HashCrownFlg = inflated
        });
    }
}
