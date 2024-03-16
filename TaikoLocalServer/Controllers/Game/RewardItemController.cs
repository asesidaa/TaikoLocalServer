namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class RewardItemController : BaseController<RewardItemController>
{
    [HttpPost("/v12r08_ww/chassis/rewarditem.php")]
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
    
    [HttpPost("/v12r00_cn/chassis/rewarditem.php")]
    [Produces("application/protobuf")]
    public IActionResult RewardItem3209([FromBody] Models.v3209.RewardItemRequest request)
    {
        Logger.LogInformation("RewardItem request : {Request}", request.Stringify());

        var response = new Models.v3209.RewardItemResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}