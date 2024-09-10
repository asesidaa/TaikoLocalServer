namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class ChallengeCompetitionController : BaseController<ChallengeCompetitionController>
{

    [HttpPost("/v12r08_ww/chassis/challengecompe_mn4g8uq1.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> HandleChallenge([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("ChallengeCompe request : {Request}", request.Stringify());

        var response = await Mediator.Send(new ChallengeCompeteQuery(request.Baid));

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/challengecompe.php")]
    [Produces("application/protobuf")]
    public IActionResult HandleChallenge3209([FromBody] Models.v3209.ChallengeCompeRequest request)
    {
        Logger.LogInformation("ChallengeCompe request : {Request}", request.Stringify());

        var response = new Models.v3209.ChallengeCompeResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}