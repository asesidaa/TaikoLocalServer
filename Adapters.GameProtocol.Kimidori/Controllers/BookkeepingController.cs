namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult FinalBookkeeping([FromBody] FinalWire.BookKeepingRequest request)
    {
        Logger.LogInformation(
            "Kimidori final bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new FinalWire.BookKeepingResponse { Result = 1 });
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation(
            "Kimidori bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
