namespace TaikoLocalServer.Application.Handlers;

public partial class RewardCardCheckQueryHandler
{
    public partial async ValueTask<CommonRewardCardCheckResponse> Handle(
        RewardCardCheckQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking Green reward card for access code {AccessCode}", request.AccessCode);
        var card = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
        return new CommonRewardCardCheckResponse
        {
            Result = 1,
            Baid = card?.Baid ?? 0
        };
    }
}
