using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/selfbest.php")]
    [Produces("application/protobuf")]
    public IActionResult SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation(
            "Momoiro selfbest.php scaffold request: Baid={Baid}, ChassisId={ChassisId}, Level={Level}, SongCount={SongCount}",
            request.Baid,
            request.ChassisId,
            request.Level,
            request.ArySongNoes?.Length ?? 0);

        return Ok(new SelfBestResponse { Result = 1 });
    }
}
