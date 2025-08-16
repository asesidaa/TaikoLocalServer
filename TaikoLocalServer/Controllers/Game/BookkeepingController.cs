namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class BookkeepingController : BaseController<BookkeepingController>
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

    [HttpPost("/v12r00_cn/chassis/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult StartupAuthCN00([FromBody] Models.CN00.BookKeepingRequest request)
    {
        Logger.LogInformation("[CN00] Bookkeeping request: {Request}", request.Stringify());
        var response = new BookKeepingResponse
        {
            Result = 1
        };
        return Ok(response);
    }
}