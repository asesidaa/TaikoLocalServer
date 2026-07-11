namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetTelopQuery(uint TelopId) : IRequest<CommonGetTelopResponse>;

public class GetTelopQueryHandler(IGameDataCatalog gameDataService, ILogger<GetTelopQueryHandler> logger)
    : IRequestHandler<GetTelopQuery, CommonGetTelopResponse>
{
    public ValueTask<CommonGetTelopResponse> Handle(GetTelopQuery request, CancellationToken cancellationToken)
    {
        var telopDataDictionary = gameDataService.GetTelopDataDictionary();
        telopDataDictionary.TryGetValue(request.TelopId, out var telopData);
        if (telopData is null)
        {
            logger.LogWarning("Telop data for telop {TelopId} not found", request.TelopId);

            return ValueTask.FromResult(new CommonGetTelopResponse
            {
                Result = 1
            });
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
