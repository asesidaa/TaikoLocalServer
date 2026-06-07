namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/challengecompe.php")]
public class ChallengeCompeController : BaseProtocolController<ChallengeCompeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Green ChallengeCompe request: {@Request}", request);
        return Ok(new ChallengeCompeResponse { Result = 1 });
    }
}
