using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/crownsdata.php")]
public sealed class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("White CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataWhite
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var white = gameDataService.White();
        var inflated = CrownsDataMappers.BuildRawInflatedBody(bestRows, white);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = white.SongHashVersion,
            HashCrownFlg = inflated
        });
    }
}
