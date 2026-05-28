namespace TaikoLocalServer.Application.Handlers;

public partial class GetTelopQueryHandler
{
    private partial ValueTask<CommonGetTelopResponse> HandleBlue(GetTelopQuery request, CancellationToken cancellationToken)
    {
        var telops = gameDataService.Blue().Telops;
        if (!telops.TryGetValue(request.TelopId, out var entry))
        {
            return ValueTask.FromResult(new CommonGetTelopResponse { Result = 1 });
        }

        return ValueTask.FromResult(new CommonGetTelopResponse
        {
            Result = 1,
            VerupNo = entry.VerupNo,
            StartDatetime = entry.StartDatetime,
            EndDatetime = entry.EndDatetime,
            Telop = entry.Message
        });
    }
}
