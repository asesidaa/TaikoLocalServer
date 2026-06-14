namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/playresult.php")]
[Route("/v08r01/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Red PlayResult request: {@Request}", request);
        Logger.LogInformation(
            "Red PlayResult challenge facts for baid {Baid}: stages={StageCount}, challenge={ChallengeCount}, user={UserCount}, bng={BngCount}",
            request.Baid,
            request.AryStageInfoes.Count,
            request.AryStageInfoes.Sum(stage => stage.AryChallengeIds.Count),
            request.AryStageInfoes.Sum(stage => stage.AryUserCompeIds.Count),
            request.AryStageInfoes.Sum(stage => stage.AryBngCompeIds.Count));
        var playResult = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Red, playResult),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
