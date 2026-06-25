using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/recommend.php")]
    [Produces("application/protobuf")]
    public IActionResult Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation(
            "Momoiro recommend.php scaffold request: ChassisId={ChassisId}, GenderType={GenderType}, PlayerAge={PlayerAge}",
            request.ChassisId,
            request.GenderType,
            request.PlayerAge);

        return Ok(new RecommendResponse { Result = 1 });
    }
}
