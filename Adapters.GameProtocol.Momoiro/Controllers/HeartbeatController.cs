using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation(
            "Momoiro heartbeat.php operational compatibility request: ChassisId={ChassisId}",
            request.ChassisId);

        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        });
    }
}
