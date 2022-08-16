using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/rewarditem.php")]
[ApiController]
public class RewardItemController : ControllerBase
{
    private readonly ILogger<RewardItemController> logger;
    public RewardItemController(ILogger<RewardItemController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardItem([FromBody] RewardItemRequest request)
    {
        logger.LogInformation("RewardItem request : {Request}", request.Stringify());

        var response = new RewardItemResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}