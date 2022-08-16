using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getsongintroduction.php")]
[ApiController]
public class GetSongIntroductionController : ControllerBase
{
    private readonly ILogger<GetSongIntroductionController> logger;
    public GetSongIntroductionController(ILogger<GetSongIntroductionController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetSongIntroduction([FromBody] GetSongIntroductionRequest request)
    {
        logger.LogInformation("GetSongIntroduction request : {Request}", request.Stringify());

        var response = new GetSongIntroductionResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}