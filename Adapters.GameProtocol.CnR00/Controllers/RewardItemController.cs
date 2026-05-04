namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class RewardItemController : BaseProtocolController<RewardItemController>
{
    [HttpPost("/v12r00_cn/chassis/rewarditem.php")]
    [Produces("application/protobuf")]
    public IActionResult RewardItemCN00([FromBody] RewardItemRequest request)
    {
        Logger.LogInformation("RewardItem request : {Request}", request.Stringify());

        var response = new RewardItemResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}
