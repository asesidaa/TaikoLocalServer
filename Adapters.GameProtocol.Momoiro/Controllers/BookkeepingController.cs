using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost(MomoiroRoutePrefixes.Final + "/bookkeeping.php")]
    [HttpPost(MomoiroRoutePrefixes.Game + "/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation(
            "Momoiro bookkeeping.php operational compatibility request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);

        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
