namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/selfbest.php")]
public sealed class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation(
            "White scaffold selfbest.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}, Level={Level}, SongCount={SongCount}",
            request.Baid,
            request.ChassisId,
            request.ShopId,
            request.Level,
            request.ArySongNoes?.Length ?? 0);
        return Ok(new SelfBestResponse { Result = 1 });
    }
}
