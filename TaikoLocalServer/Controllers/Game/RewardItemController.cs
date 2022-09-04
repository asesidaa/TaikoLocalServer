namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/rewarditem.php")]
[ApiController]
public class RewardItemController : BaseController<RewardItemController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardItem([FromBody] RewardItemRequest request)
    {
        Logger.LogInformation("RewardItem request : {Request}", request.Stringify());

        var response = new RewardItemResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}