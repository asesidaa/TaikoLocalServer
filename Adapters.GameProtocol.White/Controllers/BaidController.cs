namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/baidcheck.php")]
public sealed class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation(
            "White scaffold baidcheck.php request: DeviceType={DeviceType}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.DeviceType,
            request.ChassisId,
            request.ShopId);
        return Ok(new BAIDResponse { Result = 1 });
    }
}
