namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r08_ww/chassis/bookkeeping_s4esi5un.php")]
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