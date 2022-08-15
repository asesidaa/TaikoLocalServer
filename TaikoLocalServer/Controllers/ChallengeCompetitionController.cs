using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/challengecompe.php")]
[ApiController]
public class ChallengeCompetitionController : ControllerBase
{
    private readonly ILogger<ChallengeCompetitionController> logger;
    public ChallengeCompetitionController(ILogger<ChallengeCompetitionController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HandleChallenge([FromBody] ChallengeCompeRequest request)
    {
        logger.LogInformation("ChallengeCompe request : {Request}", request.Stringify());

        var response = new ChallengeCompeResponse
        {
            Result = 1
        };

        return Ok(response);

    }
}