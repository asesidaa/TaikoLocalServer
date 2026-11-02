using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/heartbeat.php")]
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
            GameSvrStat = 1,
            BnidSvrStat = 1,
            BanacoinStat = 1
        });
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult LegacyHeartbeat([FromBody] LegacyWire.HeartBeatRequest request)
    {
        Logger.LogInformation(
            "White legacy heartbeat.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new LegacyWire.HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        });
    }
}
