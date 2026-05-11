namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    public partial ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green Recommend stub for gender {GenderType}, age {PlayerAge}, returning empty", request.GenderType, request.PlayerAge);
        return ValueTask.FromResult(new CommonRecommendResponse());
    }
}
