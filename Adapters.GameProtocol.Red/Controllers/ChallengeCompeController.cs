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
        var response = ChallengeCompeMappers.Map(common);
        Logger.LogInformation(
            "Red ChallengeCompe response for baid {Baid}: result={Result}, challenge={ChallengeCount}/{ChallengeTrackCount}, user={UserCount}/{UserTrackCount}, bng={BngCount}/{BngTrackCount}",
            request.Baid,
            response.Result,
            response.AryChallengeStats.Count,
            response.AryChallengeStats.Sum(stat => stat.AryTrackStats.Count),
            response.AryUserCompeStats.Count,
            response.AryUserCompeStats.Sum(stat => stat.AryTrackStats.Count),
            response.AryBngCompeStats.Count,
            response.AryBngCompeStats.Sum(stat => stat.AryTrackStats.Count));
        Logger.LogDebug("Red ChallengeCompe response details for baid {Baid}: {@ChallengeStats}", request.Baid, response.AryChallengeStats);
        return Ok(response);
    }
}
