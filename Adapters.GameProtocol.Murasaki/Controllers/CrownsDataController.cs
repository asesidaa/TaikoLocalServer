using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost(MurasakiRoutePrefixes.Game + "/crownsdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Murasaki CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataMurasaki
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var murasaki = gameDataService.Murasaki();
        var inflated = CrownsDataMappers.BuildRawInflatedBody(bestRows, murasaki);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = murasaki.SongHashVersion,
            HashCrownFlg = inflated
        });
    }
}
