namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r03/chassis/bookkeeping.php")]
public class BookkeepingController : BaseController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Bookkeeping request: {Request}", request.Stringify());
        var response = new BookKeepingResponse
        {
            Result = 1
        };


        return Ok(response);
    }
}