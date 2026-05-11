namespace TaikoLocalServer.Application.Handlers;

public partial class UserDataQueryHandler
{
    private partial ValueTask<CommonUserDataResponse> HandleGreen(UserDataQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green UserData stub for baid {Baid}, returning empty success", request.Baid);
        return ValueTask.FromResult(new CommonUserDataResponse { Result = 1 });
    }
}
