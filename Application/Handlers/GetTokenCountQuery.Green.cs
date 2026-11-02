namespace TaikoLocalServer.Application.Handlers;

public partial class GetTokenCountQueryHandler
{
    private partial ValueTask<CommonGetTokenCountResponse> HandleGreen(GetTokenCountQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetTokenCount stub for baid {Baid}, returning empty", request.Baid);
        return ValueTask.FromResult(new CommonGetTokenCountResponse { Result = 1 });
    }
}
