namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost(MurasakiRoutePrefixes.Final + "/bookkeeping.php")]
    [HttpPost(MurasakiRoutePrefixes.Compatibility + "/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation(
            "Murasaki bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
