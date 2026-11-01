using Throw;

namespace TaikoLocalServer.Application.Handlers;

/// <param name="ChassisId">The asking cabinet: one with its own settings gets its own Dani Dojo course set.</param>
public readonly record struct GetDanOdaiQuery(uint[] DanIds, uint Type, string ChassisId) : IRequest<List<DanData>>;

public class GetDanOdaiQueryHandler : IRequestHandler<GetDanOdaiQuery, List<DanData>>
{
    private readonly IGameDataCatalog gameDataService;
    private readonly ICabinetRegistry cabinets;

    public GetDanOdaiQueryHandler(IGameDataCatalog gameDataService, ICabinetRegistry cabinets)
    {
        this.gameDataService = gameDataService;
        this.cabinets = cabinets;
    }

    public ValueTask<List<DanData>> Handle(GetDanOdaiQuery request, CancellationToken cancellationToken)
    {
        var type = (DanType)request.Type;
        type.Throw().IfOutOfRange();
        var danDataList = new List<DanData>();
        switch (type)
        {
            case DanType.Normal:
                var danDataDictionary = cabinets.Find(request.ChassisId) is { OwnSettings: true } cabinet
                    ? gameDataService.GetDanSet(cabinet.DanSet)
                    : gameDataService.GetCommonDanDataDictionary();

                foreach (var danId in request.DanIds)
                {
                    if (danDataDictionary.TryGetValue(danId, out var danData))
                    {
                        danDataList.Add(danData);
                    }
                }
                break;
            case DanType.Gaiden:
                var gaidenDataDictionary = gameDataService.GetCommonGaidenDataDictionary();
                
                foreach (var danId in request.DanIds)
                {
                    if (gaidenDataDictionary.TryGetValue(danId, out var danData))
                    {
                        danDataList.Add(danData);
                    }
                }
                break;
            default:
                throw new ApplicationException("Impossible");
        }

        return ValueTask.FromResult(danDataList);
    }
}