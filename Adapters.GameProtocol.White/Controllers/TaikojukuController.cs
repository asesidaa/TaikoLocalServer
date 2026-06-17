namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/taikojuku.php")]
public sealed class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation(
            "White scaffold taikojuku.php request: ChassisId={ChassisId}, ShopId={ShopId}, GetDanCount={GetDanCount}",
            request.ChassisId,
            request.ShopId,
            request.GetDans?.Length ?? 0);
        return Ok(new TaikojukuResponse { Result = 1 });
    }
}
