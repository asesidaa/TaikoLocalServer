using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation(
            "White scaffold bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult LegacyBookkeeping([FromBody] LegacyWire.BookKeepingRequest request)
    {
        Logger.LogInformation(
            "White legacy bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new LegacyWire.BookKeepingResponse { Result = 1 });
    }
}
