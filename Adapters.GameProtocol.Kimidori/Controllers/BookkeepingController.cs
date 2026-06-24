namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class BookkeepingController : BaseProtocolController<BookkeepingController>
{
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
