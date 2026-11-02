using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Compression;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/crownsdata.php")]
public class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Blue CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataBlue
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var blue = gameDataService.Blue();
        var inflated = BlueCrownResponseBuilder.BuildInflatedBody(bestRows, blue);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = blue.SongHashVersion,
            HashCrownFlg = GZipBytesUtil.GetGZipBytes(inflated)
        });
    }
}
