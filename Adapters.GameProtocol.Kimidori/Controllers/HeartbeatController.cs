namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult FinalHeartbeat([FromBody] FinalWire.HeartBeatRequest request)
    {
        Logger.LogInformation("Kimidori final heartbeat.php request: {@Request}", request);
        return Ok(new FinalWire.HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        });
    }

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
