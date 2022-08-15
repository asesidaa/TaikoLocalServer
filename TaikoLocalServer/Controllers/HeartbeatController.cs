namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/heartbeat.php")]
public class HeartbeatController : ControllerBase
{
    private readonly ILogger<HeartbeatController> logger;
    
    public HeartbeatController(ILogger<HeartbeatController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HeartBeat([FromBody] HeartBeatRequest request)
    {
        logger.LogInformation("Heartbeat request: {Request}", request.Stringify());
        var response = new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        };

        return Ok(response);
    }
}