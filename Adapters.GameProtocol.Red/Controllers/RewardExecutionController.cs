namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/rewardexecution.php")]
[Route("/v08r01/chassis/rewardexecution.php")]
public class RewardExecutionController : BaseProtocolController<RewardExecutionController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardExecution([FromBody] RewardexecutionRequest request)
    {
        Logger.LogInformation("Red route probe rewardexecution.php request: {@Request}", request);
        return Ok(new RewardexecutionResponse { Result = 1 });
    }
}
