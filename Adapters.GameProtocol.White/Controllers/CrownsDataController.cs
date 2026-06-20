using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Application.Abstractions;
using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/crownsdata.php")]
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

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/crownsdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyCrownsData([FromBody] LegacyWire.CrownsDataRequest request)
    {
        Logger.LogInformation("White legacy CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataWhite
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var white = gameDataService.White();
        var inflated = CrownsDataMappers.BuildRawInflatedBody(bestRows, white);

        return Ok(new LegacyWire.CrownsDataResponse
        {
            Result = 1,
            SongHashVer = white.SongHashVersion,
            HashCrownFlg = inflated
        });
    }
}
