namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class BookkeepingController : BaseController<BookkeepingController>
{
    [HttpPost("/v12r08_ww/chassis/bookkeeping_s4esi5un.php")]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("[3906] Bookkeeping request: {Request}", request.Stringify());
        var response = new BookKeepingResponse
        {
            Result = 1
        };


        return Ok(response);
    }

    [HttpPost("/v12r00_cn/chassis/bookkeeping.php")]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth3209([FromBody] Models.v3209.BookKeepingRequest request)
    {
        Logger.LogInformation("[3209] Bookkeeping request: {Request}", request.Stringify());
        var response = new BookKeepingResponse
        {
            Result = 1
        };
        return Ok(response);
    }
}