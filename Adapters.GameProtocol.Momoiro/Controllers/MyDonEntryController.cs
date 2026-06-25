using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/mydonentry.php")]
    [Produces("application/protobuf")]
    public IActionResult MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation(
            "Momoiro mydonentry.php scaffold request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);

        return Ok(new MydonEntryResponse { Result = 1 });
    }
}
