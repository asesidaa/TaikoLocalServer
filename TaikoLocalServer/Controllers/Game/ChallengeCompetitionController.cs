namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class ChallengeCompetitionController : BaseController<ChallengeCompetitionController>
{
    [HttpPost("/v12r08_ww/chassis/challengecompe.php")]
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