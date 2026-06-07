namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/rewardcardcheck.php")]
public class RewardCardCheckController : BaseProtocolController<RewardCardCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> RewardCardCheck([FromBody] RewardcardcheckRequest request)
    {
        Logger.LogInformation("Green RewardCardCheck request: {@Request}", request);
        var common = await Mediator.Send(new RewardCardCheckQuery(request.AccessCode), HttpContext.RequestAborted);
        return Ok(new RewardcardcheckResponse
        {
            Result = common.Result,
            Baid = common.Baid
        });
    }
}
