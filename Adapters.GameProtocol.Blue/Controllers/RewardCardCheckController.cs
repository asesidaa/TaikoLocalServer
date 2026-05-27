namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/rewardcardcheck.php")]
public class RewardCardCheckController : BaseProtocolController<RewardCardCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> RewardCardCheck([FromBody] RewardcardcheckRequest request)
    {
        Logger.LogInformation("Blue RewardCardCheck request: {Request}", request.Stringify());
        var common = await Mediator.Send(new RewardCardCheckQuery(request.AccessCode), HttpContext.RequestAborted);
        return Ok(new RewardcardcheckResponse
        {
            Result = common.Result,
            Baid = common.Baid
        });
    }
}
