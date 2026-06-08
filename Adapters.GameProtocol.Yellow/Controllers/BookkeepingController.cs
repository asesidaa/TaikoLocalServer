namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/bookkeeping.php")]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Yellow Bookkeeping request from {ChassisId}", request.ChassisId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}
