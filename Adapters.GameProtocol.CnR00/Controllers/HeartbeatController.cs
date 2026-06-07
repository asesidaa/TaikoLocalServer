namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost("/v12r00_cn/chassis/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult HeartBeatCN00([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Heartbeat request: {@Request}", request);
        var response = new HeartBeatResponse
        {
            Result = 1,
            GameSvrStat = 1
        };

        return Ok(response);
    }
}
