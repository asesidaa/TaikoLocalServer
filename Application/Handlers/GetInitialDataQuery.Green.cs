namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleGreen(GetInitialDataQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green InitialData stub returning minimal success");

        return ValueTask.FromResult(new CommonInitialDataCheckResponse
        {
            Result = 1,
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        });
    }
}
