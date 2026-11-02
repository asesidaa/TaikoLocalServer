namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/challengecompe.php")]
public class ChallengeCompeController : BaseProtocolController<ChallengeCompeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Yellow ChallengeCompe request: {@Request}", request);
        var common = await Mediator.Send(
            new GetChallengeCompeQuery(GameEra.Yellow, request.Baid),
            HttpContext.RequestAborted);
        return Ok(ChallengeCompeMappers.Map(common));
    }
}
