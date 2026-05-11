namespace TaikoLocalServer.Application.Handlers;

public partial class GetGhostDataQueryHandler
{
    public partial ValueTask<CommonGhostDataResponse> Handle(GetGhostDataQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetGhostData stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonGhostDataResponse());
    }
}
