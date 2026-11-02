namespace TaikoLocalServer.Application.Handlers;

public partial class GetTelopQueryHandler
{
    private partial ValueTask<CommonGetTelopResponse> HandleNijiiro(GetTelopQuery request, CancellationToken cancellationToken)
    {
        if (!gameDataService.Nijiiro().GetTelopDataDictionary().TryGetValue(request.TelopId, out var telopData))
        {
            logger.LogWarning("Telop data for telop {TelopId} not found", request.TelopId);
            return ValueTask.FromResult(new CommonGetTelopResponse { Result = 1 });
        }

        return ValueTask.FromResult(new CommonGetTelopResponse
        {
            Result = 1,
            VerupNo = telopData.VerupNo,
            StartDatetime = telopData.StartDatetime,
            EndDatetime = telopData.EndDatetime,
            Telop = telopData.Telop
        });
    }
}
