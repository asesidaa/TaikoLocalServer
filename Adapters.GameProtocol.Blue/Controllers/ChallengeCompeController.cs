namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/challengecompe.php")]
public class ChallengeCompeController : BaseProtocolController<ChallengeCompeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Blue ChallengeCompe request: {Request}", request.Stringify());
        return Ok(new ChallengeCompeResponse { Result = 1 });
    }
}
