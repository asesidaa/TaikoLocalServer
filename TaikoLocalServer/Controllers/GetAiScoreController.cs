using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getaiscore.php")]
[ApiController]
public class GetAiScoreController : ControllerBase
{
    private readonly ILogger<GetAiScoreController> logger;
    public GetAiScoreController(ILogger<GetAiScoreController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetAiScore([FromBody] GetAiScoreRequest request)
    {
        logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var response = new GetAiScoreResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}