namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/bookkeeping.php")]
public class BookkeepingController : ControllerBase
{
    private readonly ILogger<BookkeepingController> logger;
    public BookkeepingController(ILogger<BookkeepingController> logger) {
        this.logger = logger;
    }
    
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] BookKeepingRequest request)
    {
        logger.LogInformation("Bookkeeping request: {Request}", request.Stringify());
        var response = new BookKeepingResponse()
        {
            Result = 1
        };
        

        return Ok(response);
    }
    
}