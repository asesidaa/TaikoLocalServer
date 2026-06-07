namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/crownsdata.php")]
public class CrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Green CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataGreen
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var green = gameDataService.Green();
        var inflated = GreenCrownResponseBuilder.BuildInflatedBody(bestRows, green);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = green.SongHashVersion,
            HashCrownFlg = GZipBytesUtil.GetGZipBytes(inflated)
        });
    }
}
