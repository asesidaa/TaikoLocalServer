using Microsoft.AspNetCore.Http;
using Swan.Extensions;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/gettelop.php")]
[ApiController]
public class GetTelopController : ControllerBase
{
    private readonly ILogger<GetTelopController> logger;
    public GetTelopController(ILogger<GetTelopController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetTelop([FromBody] GettelopRequest request)
    {
        logger.LogInformation("GetTelop request : {Request}", request.Stringify());

        var startDateTime = DateTime.Now - TimeSpan.FromDays(999.0);
        var endDateTime = DateTime.Now + TimeSpan.FromDays(999.0);
        
        var response = new GettelopResponse
        {
            Result = 1,
            StartDatetime = startDateTime.ToUnixTimeMilliseconds().ToString(),
            EndDatetime = endDateTime.ToUnixTimeMilliseconds().ToString(),
            Telop = "Hello world",
            VerupNo = 1
        };

        return Ok(response);
    }
}