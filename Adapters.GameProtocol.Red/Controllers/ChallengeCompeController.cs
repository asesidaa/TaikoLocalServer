namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/challengecompe.php")]
[Route("/v08r01/chassis/challengecompe.php")]
public class ChallengeCompeController : BaseProtocolController<ChallengeCompeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Red ChallengeCompe request: {@Request}", request);
        var common = await Mediator.Send(new GetChallengeCompeQuery(GameEra.Red, request.Baid), HttpContext.RequestAborted);
        return Ok(ChallengeCompeMappers.Map(common));
    }
}
