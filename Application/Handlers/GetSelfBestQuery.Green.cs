namespace TaikoLocalServer.Application.Handlers;

public partial class GetSelfBestQueryHandler
{
    private partial ValueTask<CommonSelfBestResponse> HandleGreen(GetSelfBestQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetSelfBest stub for baid {Baid}, returning empty", request.Baid);

        return ValueTask.FromResult(new CommonSelfBestResponse
        {
            Result = 1,
            Level = request.Difficulty
        });
    }
}
