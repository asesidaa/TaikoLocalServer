namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost("/v12r08_ww/chassis/bookkeeping_s4esi5un.php")]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("[WW08] Bookkeeping request: {Request}", request.Stringify());
        var response = new BookKeepingResponse
        {
            Result = 1
        };


        return Ok(response);
    }
}
