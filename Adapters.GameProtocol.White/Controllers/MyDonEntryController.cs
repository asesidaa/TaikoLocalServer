namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/mydonentry.php")]
public sealed class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation(
            "White scaffold mydonentry.php request: DeviceType={DeviceType}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.DeviceType,
            request.ChassisId,
            request.ShopId);
        return Ok(new MydonEntryResponse { Result = 1 });
    }
}
