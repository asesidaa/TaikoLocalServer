namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial ValueTask<CommonBaidResponse> HandleGreen(BaidQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green BaidQuery stub for access code {AccessCode}, returning new-user success", request.AccessCode);

        return ValueTask.FromResult(new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = true
        });
    }
}
