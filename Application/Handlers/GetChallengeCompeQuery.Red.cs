namespace TaikoLocalServer.Application.Handlers;

public partial class GetChallengeCompeQueryHandler
{
    private partial ValueTask<CommonChallengeCompeResponse> HandleRed(
        GetChallengeCompeQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Red ChallengeCompe ignored for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonChallengeCompeResponse());
    }
}
