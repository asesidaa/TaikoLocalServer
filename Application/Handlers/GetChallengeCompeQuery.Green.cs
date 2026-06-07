namespace TaikoLocalServer.Application.Handlers;

public partial class GetChallengeCompeQueryHandler
{
    private partial ValueTask<CommonChallengeCompeResponse> HandleGreen(GetChallengeCompeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green ChallengeCompe stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonChallengeCompeResponse());
    }
}
