using Microsoft.AspNetCore.Http;

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
        logger.LogInformation("GetTelopController request : {Request}", request.Stringify());

        var response = new GettelopResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}