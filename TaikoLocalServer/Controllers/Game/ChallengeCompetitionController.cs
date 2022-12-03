namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/challengecompe.php")]
[ApiController]
public class ChallengeCompetitionController : BaseController<ChallengeCompetitionController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HandleChallenge([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("ChallengeCompe request : {Request}", request.Stringify());

        var response = new ChallengeCompeResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}