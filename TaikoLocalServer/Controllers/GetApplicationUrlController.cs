using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getapplicationurl.php")]
[ApiController]
public class GetApplicationUrlController : ControllerBase
{
    private const string ApplicationUrl = "vsapi.taiko-p.jp";
    
    private readonly ILogger<GetApplicationUrlController> logger;
    public GetApplicationUrlController(ILogger<GetApplicationUrlController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetApplicationUrl([FromBody] GetApplicationUrlRequest request)
    {
        logger.LogInformation("GetApplicationUrl request : {Request}", request.Stringify());

        var response = new GetApplicationUrlResponse
        {
            Result = 1,
            ApplicationUrl = ApplicationUrl
        };

        return Ok(response);
    }
}