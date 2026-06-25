using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/playresult.php")]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation(
            "Momoiro playresult.php scaffold request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}, StageCount={StageCount}",
            request.Baid,
            request.ChassisId,
            request.ShopId,
            request.AryStageInfoes.Count);

        return Ok(new PlayResultResponse { Result = 1 });
    }
}
