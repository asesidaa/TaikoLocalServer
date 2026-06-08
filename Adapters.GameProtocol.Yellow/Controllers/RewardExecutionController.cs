namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/rewardexecution.php")]
public class RewardExecutionController : BaseProtocolController<RewardExecutionController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardExecution([FromBody] RewardexecutionRequest request)
    {
        Logger.LogInformation("Yellow RewardExecution request from {ChassisId}", request.ChassisId);
        return Ok(new RewardexecutionResponse { Result = 1 });
    }
}
