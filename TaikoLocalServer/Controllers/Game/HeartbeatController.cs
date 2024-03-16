namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class HeartbeatController : BaseController<HeartbeatController>
{
    [HttpPost("/v12r08_ww/chassis/heartbeat_hcv5akgr.php")]
    [Produces("application/protobuf")]
    public IActionResult HeartBeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Heartbeat request: {Request}", request.Stringify());
        var response = new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        };

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/heartbeat.php")]
    [Produces("application/protobuf")]
    public IActionResult HeartBeat3209([FromBody] Models.v3209.HeartBeatRequest request)
    {
        Logger.LogInformation("Heartbeat request: {Request}", request.Stringify());
        var response = new Models.v3209.HeartBeatResponse
        {
            Result = 1,
            GameSvrStat = 1
        };

        return Ok(response);
    }
}