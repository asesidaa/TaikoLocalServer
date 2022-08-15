using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getscorerank.php")]
[ApiController]
public class GetScoreRankController : ControllerBase
{
    private readonly ILogger<GetScoreRankController> logger;
    public GetScoreRankController(ILogger<GetScoreRankController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetScoreRank([FromBody] GetScoreRankRequest request)
    {
        logger.LogInformation("GetScoreRankController request : {Request}", request.Stringify());
        var response = new GetScoreRankResponse
        {
            Result = 1
        };
        
        return Ok(response);
    }
}