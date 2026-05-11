namespace TaikoLocalServer.Application.Handlers;

public partial class RewardExecutionCommandHandler
{
    public partial ValueTask<CommonRewardExecutionResponse> Handle(RewardExecutionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green RewardExecution stub for baid {Baid}, returning success", request.Baid);
        return ValueTask.FromResult(new CommonRewardExecutionResponse());
    }
}
