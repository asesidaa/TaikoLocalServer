namespace TaikoLocalServer.Application.Handlers;

public partial class GetGhostScoreQueryHandler
{
    public partial ValueTask<CommonGhostScoreResponse> Handle(GetGhostScoreQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetGhostScore stub for baid {Baid}, song {SongNo}, returning empty", request.Baid, request.SongNo);
        return ValueTask.FromResult(new CommonGhostScoreResponse());
    }
}
