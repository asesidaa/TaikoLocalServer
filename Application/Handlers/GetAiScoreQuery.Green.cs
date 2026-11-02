namespace TaikoLocalServer.Application.Handlers;

public partial class GetAiScoreQueryHandler
{
    private partial ValueTask<CommonAiScoreResponse> HandleGreen(GetAiScoreQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetAiScore stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonAiScoreResponse { Result = 1 });
    }
}
