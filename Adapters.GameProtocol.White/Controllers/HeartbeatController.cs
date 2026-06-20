namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/heartbeat.php")]
[Route(WhiteRoutePrefixes.Compatibility + "/heartbeat.php")]
public sealed class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation(
            "White scaffold heartbeat.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        });
    }
}
