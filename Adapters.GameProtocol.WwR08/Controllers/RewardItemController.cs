namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class RewardItemController : BaseProtocolController<RewardItemController>
{
    [HttpPost("/v12r08_ww/chassis/rewarditem.php")]
    [Produces("application/protobuf")]
    public IActionResult RewardItem([FromBody] RewardItemRequest request)
    {
        Logger.LogInformation("RewardItem request : {@Request}", request);

        var response = new RewardItemResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}
