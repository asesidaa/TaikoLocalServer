namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/playresult.php")]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation(
            "White scaffold playresult.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}, PlayMode={PlayMode}, StageCount={StageCount}",
            request.Baid,
            request.ChassisId,
            request.ShopId,
            request.PlayMode,
            request.AryStageInfoes.Count);
        return Ok(new PlayResultResponse { Result = 1 });
    }
}
