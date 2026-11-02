namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/heartbeat.php")]
public class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Green Heartbeat request: {@Request}", request);
        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1,
            BnidSvrStat = 1,
            BanacoinStat = 1
        });
    }
}
