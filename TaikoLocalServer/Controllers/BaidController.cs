using Microsoft.AspNetCore.Mvc;
using Swan.Formatters;
using taiko.game;

namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/baidcheck.php")]
public class BaidController:ControllerBase
{
    private readonly ILogger<BaidController> logger;

    public BaidController(ILogger<BaidController> logger)
    {
        this.logger = logger;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] BAIDRequest request)
    {
        logger.LogInformation("Heartbeat request: {Request}", request.Stringify());
        var response = new BAIDResponse()
        {
            Result = 0
        };

        return Ok(response);
    }

}