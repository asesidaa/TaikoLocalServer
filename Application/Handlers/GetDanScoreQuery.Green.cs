namespace TaikoLocalServer.Application.Handlers;

public partial class GetDanScoreQueryHandler
{
    private partial ValueTask<CommonDanScoreDataResponse> HandleGreen(GetDanScoreQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetDanScore stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonDanScoreDataResponse { Result = 1 });
    }
}
