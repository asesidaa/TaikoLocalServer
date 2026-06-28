using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/crownsdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalCrownsData([FromBody] FinalWire.CrownsDataRequest request)
    {
        Logger.LogInformation("Kimidori final CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataKimidori
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var kimidori = gameDataService.Kimidori();
        var hashIndexed = CrownsDataMappers.BuildHashIndexedBody(bestRows, kimidori);

        return Ok(new FinalWire.CrownsDataResponse
        {
            Result = 1,
            SongHashVer = kimidori.SongHashVersion,
            HashCrownFlg = hashIndexed
        });
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/crownsdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Kimidori CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataKimidori
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var kimidori = gameDataService.Kimidori();
        var hashIndexed = CrownsDataMappers.BuildHashIndexedBody(bestRows, kimidori);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = kimidori.SongHashVersion,
            HashCrownFlg = hashIndexed
        });
    }
}
