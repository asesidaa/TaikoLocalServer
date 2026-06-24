namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Kimidori heartbeat.php request: {@Request}", request);
        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        });
    }
}
