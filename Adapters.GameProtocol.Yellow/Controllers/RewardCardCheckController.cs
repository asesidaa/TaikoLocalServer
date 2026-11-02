namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/rewardcardcheck.php")]
public class RewardCardCheckController : BaseProtocolController<RewardCardCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> RewardCardCheck([FromBody] RewardcardcheckRequest request)
    {
        Logger.LogInformation("Yellow RewardCardCheck request: {@Request}", request);
        var common = await Mediator.Send(new RewardCardCheckQuery(request.AccessCode), HttpContext.RequestAborted);
        return Ok(new RewardcardcheckResponse
        {
            Result = common.Result,
            Baid = common.Baid
        });
    }
}
