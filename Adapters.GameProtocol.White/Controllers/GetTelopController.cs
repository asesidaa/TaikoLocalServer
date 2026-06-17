namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/gettelop.php")]
public sealed class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation(
            "White scaffold gettelop.php request: ChassisId={ChassisId}, ShopId={ShopId}, TelopId={TelopId}",
            request.ChassisId,
            request.ShopId,
            request.TelopId);
        return Ok(new GettelopResponse { Result = 1 });
    }
}
