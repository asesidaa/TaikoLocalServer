namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/challengecompe.php")]
[Route("/v08r01/chassis/challengecompe.php")]
public class ChallengeCompeController : BaseProtocolController<ChallengeCompeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Red route probe challengecompe.php request: {@Request}", request);
        return Ok(new ChallengeCompeResponse { Result = 1 });
    }
}
