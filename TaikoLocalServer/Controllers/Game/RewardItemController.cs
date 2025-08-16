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
    public IActionResult RewardItemCN00([FromBody] Models.CN00.RewardItemRequest request)
    {
        Logger.LogInformation("RewardItem request : {Request}", request.Stringify());

        var response = new Models.CN00.RewardItemResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}