namespace TaikoLocalServer.Application.Handlers;

public partial class GetChallengeCompeQueryHandler
{
    public partial ValueTask<CommonChallengeCompeResponse> Handle(GetChallengeCompeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green ChallengeCompe stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonChallengeCompeResponse());
    }
}
