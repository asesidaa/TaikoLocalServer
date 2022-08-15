using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getaidata.php")]
[ApiController]
public class GetAiDataController : ControllerBase
{
    private readonly ILogger<GetAiDataController> logger;
    public GetAiDataController(ILogger<GetAiDataController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetAiData([FromBody] GetAiDataRequest request)
    {
        logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var response = new GetAiDataResponse
        {
            Result  = 1
        };

        return Ok(response);
    }
}