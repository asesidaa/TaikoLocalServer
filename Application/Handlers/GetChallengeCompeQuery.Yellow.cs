namespace TaikoLocalServer.Application.Handlers;

public partial class GetChallengeCompeQueryHandler
{
    private partial ValueTask<CommonChallengeCompeResponse> HandleYellow(
        GetChallengeCompeQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Yellow ChallengeCompe stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonChallengeCompeResponse());
    }
}
