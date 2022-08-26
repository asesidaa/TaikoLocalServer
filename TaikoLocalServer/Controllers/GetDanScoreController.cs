namespace TaikoLocalServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GetDanScoreController : ControllerBase
{
    private readonly ILogger<GetDanScoreController> logger;
    public GetDanScoreController(ILogger<GetDanScoreController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetDanScore([FromBody] GetDanScoreRequest request)
    {
        logger.LogInformation("GetDanScore request : {Request}", request.Stringify());

        var response = new GetDanScoreResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}