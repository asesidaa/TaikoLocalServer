namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        var common = PlayResultMappers.Map(request);
        Logger.LogInformation(
            "Blue PlayResult request: baid={Baid} chassis={ChassisId} shop={ShopId} play_datetime={PlayDatetime} stages={StageCount} battle_stage={BattleStage} release_battle={ReleaseBattle} tokkun={Tokkun}",
            request.Baid,
            request.ChassisId,
            request.ShopId,
            request.PlayDatetime,
            request.AryStageInfoes.Count,
            common.HasBattleStageData,
            common.HasReleaseBattleData,
            common.HasTokkunStageInfo);

        var result = await Mediator.Send(
            new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
