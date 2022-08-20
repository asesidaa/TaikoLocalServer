using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/gettokencount.php")]
[ApiController]
public class GetTokenCountController : ControllerBase
{
    private readonly ILogger<GetTokenCountController> logger;
    public GetTokenCountController(ILogger<GetTokenCountController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetTokenCount([FromBody] GetTokenCountRequest request)
    {
        logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var response = new GetTokenCountResponse
        {
            Result = 1
        };
        
        response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
        {
            TokenCount = 10,
            TokenId = 1
        });

        return Ok(response);
    }
}