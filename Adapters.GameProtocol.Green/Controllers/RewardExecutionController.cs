namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/rewardexecution.php")]
public class RewardExecutionController : BaseProtocolController<RewardExecutionController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> RewardExecution([FromBody] RewardexecutionRequest request)
    {
        Logger.LogInformation("Green RewardExecution request: {Request}", request.Stringify());
        var common = await Mediator.Send(new RewardExecutionCommand(
            request.Baid,
            request.ReleaseSongNoes ?? [],
            request.GetToneNoes ?? [],
            request.GetCostumeNo1s ?? [],
            request.GetCostumeNo2s ?? [],
            request.GetCostumeNo3s ?? [],
            request.GetCostumeNo4s ?? [],
            request.GetCostumeNo5s ?? [],
            request.GetTitleNoes ?? []), HttpContext.RequestAborted);

        return Ok(new RewardexecutionResponse { Result = common.Result });
    }
}
