namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/rewardcardcheck.php")]
[Route("/v08r01/chassis/rewardcardcheck.php")]
public class RewardCardCheckController : BaseProtocolController<RewardCardCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardCardCheck([FromBody] RewardcardcheckRequest request)
    {
        Logger.LogInformation("Red route probe rewardcardcheck.php request: {@Request}", request);
        return Ok(new RewardcardcheckResponse { Result = 1 });
    }
}
