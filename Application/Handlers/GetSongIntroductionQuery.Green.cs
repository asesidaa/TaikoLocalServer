namespace TaikoLocalServer.Application.Handlers;

public partial class GetSongIntroductionQueryHandler
{
    private partial ValueTask<CommonGetSongIntroductionResponse> HandleGreen(GetSongIntroductionQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetSongIntroduction stub for {Count} set ids, returning empty", request.SetIds.Length);
        return ValueTask.FromResult(new CommonGetSongIntroductionResponse { Result = 1 });
    }
}
