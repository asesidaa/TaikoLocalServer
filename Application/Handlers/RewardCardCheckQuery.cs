namespace TaikoLocalServer.Application.Handlers;

public readonly record struct RewardCardCheckQuery(string AccessCode) : IRequest<CommonRewardCardCheckResponse>;

public partial class RewardCardCheckQueryHandler(ILogger<RewardCardCheckQueryHandler> logger)
    : IRequestHandler<RewardCardCheckQuery, CommonRewardCardCheckResponse>
{
    public partial ValueTask<CommonRewardCardCheckResponse> Handle(RewardCardCheckQuery request, CancellationToken cancellationToken);
}
