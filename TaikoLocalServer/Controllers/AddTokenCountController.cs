namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/addtokencount.php")]
[ApiController]
public class AddTokenCountController : ControllerBase
{
    private readonly ILogger<AddTokenCountController> logger;
    public AddTokenCountController(ILogger<AddTokenCountController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult AddTokenCount([FromBody] AddTokenCountRequest request)
    {
        logger.LogInformation("AddTokenCount request : {Request}", request.Stringify());

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}