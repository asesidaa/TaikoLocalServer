namespace TaikoLocalServer.Application.Handlers;

public partial class RewardCardCheckQueryHandler
{
    public partial ValueTask<CommonRewardCardCheckResponse> Handle(RewardCardCheckQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green RewardCardCheck stub for access code {AccessCode}, returning success", request.AccessCode);
        return ValueTask.FromResult(new CommonRewardCardCheckResponse());
    }
}
