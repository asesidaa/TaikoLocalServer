using TaikoLocalServer.Adapters.GameProtocol.Shared.Cabinets;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class HeartbeatController(IServerAccess access) : BaseProtocolController<HeartbeatController>
{
    [HttpPost("/v12r08_ww/chassis/heartbeat_hcv5akgr.php")]
    [Produces("application/protobuf")]
    public IActionResult HeartBeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Heartbeat request: {@Request}", request);
        var response = new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = access.GameSvrStat()
        };

        return Ok(response);
    }
}
