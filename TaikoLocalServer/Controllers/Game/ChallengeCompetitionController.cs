namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class ChallengeCompetitionController : BaseController<ChallengeCompetitionController>
{

    [HttpPost("/v12r08_ww/chassis/challengecompe_mn4g8uq1.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> HandleChallenge([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("ChallengeCompe request : {Request}", request.Stringify());

        CommonChallengeCompeResponse response = await Mediator.Send(new ChallengeCompeteQuery(request.Baid));
        var response_3906 = Mappers.ChallengeCompeMappers.MapTo3906(response);

        return Ok(response_3906);
    }
    
    [HttpPost("/v12r00_cn/chassis/challengecompe.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> HandleChallenge3209([FromBody] Models.v3209.ChallengeCompeRequest request)
    {
        Logger.LogInformation("ChallengeCompe request : {Request}", request.Stringify());

        CommonChallengeCompeResponse response = await Mediator.Send(new ChallengeCompeteQuery((uint)request.Baid));
        var response_3209 = Mappers.ChallengeCompeMappers.MapTo3209(response);

        return Ok(response_3209);
    }
}