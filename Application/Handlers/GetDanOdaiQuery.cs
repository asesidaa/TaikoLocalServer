using Throw;

namespace TaikoLocalServer.Application.Handlers;

/// <param name="ChassisId">Nijiiro: the asking cabinet; one with its own settings gets its own Dani Dojo course set.</param>
public readonly record struct GetDanOdaiQuery(GameEra Era, uint[] DanIds, uint Type, string ChassisId = "") : IRequest<List<DanData>>;

public partial class GetDanOdaiQueryHandler : IRequestHandler<GetDanOdaiQuery, List<DanData>>
{
    private readonly IGameDataCatalog gameDataService;
    private readonly ICabinetRegistry cabinets;

    public GetDanOdaiQueryHandler(IGameDataCatalog gameDataService, ICabinetRegistry cabinets)
    {
        this.gameDataService = gameDataService;
        this.cabinets = cabinets;
    }

    public ValueTask<List<DanData>> Handle(GetDanOdaiQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<List<DanData>> HandleNijiiro(GetDanOdaiQuery request, CancellationToken cancellationToken);
    private partial ValueTask<List<DanData>> HandleGreen(GetDanOdaiQuery request, CancellationToken cancellationToken);
}
