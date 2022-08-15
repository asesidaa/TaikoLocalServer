using taiko.vsinterface;

namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/initialdatacheck.php")]
public class InitialDataCheckController:ControllerBase
{
    private readonly ILogger<InitialDataCheckController> logger;

    public InitialDataCheckController(ILogger<InitialDataCheckController> logger)
    {
        this.logger = logger;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        logger.LogInformation("Heartbeat request: {Request}", request.Stringify());
        var response = new InitialdatacheckResponse
        {
            Result = 1,
            IsDanplay = true,
            IsAibattle = true,
            IsClose = false
        };

        return Ok(response);
    }

}