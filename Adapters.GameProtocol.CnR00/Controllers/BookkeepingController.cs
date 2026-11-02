namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost("/v12r00_cn/chassis/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult StartupAuthCN00([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("[CN00] Bookkeeping request: {@Request}", request);
        var response = new BookKeepingResponse
        {
            Result = 1
        };
        return Ok(response);
    }
}
