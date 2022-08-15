using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getdanodai.php")]
[ApiController]
public class GetDanOdaiController : ControllerBase
{
    private readonly ILogger<GetDanOdaiController> logger;
    public GetDanOdaiController(ILogger<GetDanOdaiController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetDanOdai([FromBody] GetDanOdaiRequest request)
    {
        logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());

        var response = new GetDanOdaiResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}