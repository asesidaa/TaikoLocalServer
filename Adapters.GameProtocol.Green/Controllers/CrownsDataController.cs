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
        Logger.LogInformation("Green CrownsData request: {Request}", request.Stringify());
        var bestRows = await context.SongBestDataGreen
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var inflated = GreenCrownResponseBuilder.BuildInflatedBody(bestRows);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = gameDataService.Green().SongHashVersion,
            HashCrownFlg = GreenProtocolBytes.CompressZlib(inflated)
        });
    }
}
