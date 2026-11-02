namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class ChallengeCompetitionController : BaseProtocolController<ChallengeCompetitionController>
{
    [HttpPost("/v12r08_ww/chassis/challengecompe.php")]
    [Produces("application/protobuf")]
    public IActionResult HandleChallenge([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("ChallengeCompe request : {@Request}", request);

        var response = new ChallengeCompeResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}
