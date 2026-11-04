using Throw;

namespace TaikoLocalServer.Application.Handlers;

/// <param name="ChassisId">The asking cabinet: Nijiiro reads back its dan set's results only.</param>
public readonly record struct GetDanScoreQuery(uint Baid, GameEra Era, uint Type, uint[] DanIds, string ChassisId = "") : IRequest<CommonDanScoreDataResponse>;

public partial class GetDanScoreQueryHandler : IRequestHandler<GetDanScoreQuery, CommonDanScoreDataResponse>
{
    private readonly ILogger<GetDanScoreQueryHandler> logger;
    private readonly ITaikoDbContext                   context;
    private readonly IGameDataCatalog gameDataService;
    private readonly ICabinetRegistry? cabinets;


    public GetDanScoreQueryHandler(
        ILogger<GetDanScoreQueryHandler> logger,
        ITaikoDbContext context,
        IGameDataCatalog gameDataService,
        ICabinetRegistry? cabinets = null)
    {
        this.logger = logger;
        this.context = context;
        this.gameDataService = gameDataService;
        this.cabinets = cabinets;
    }

    public ValueTask<CommonDanScoreDataResponse> Handle(GetDanScoreQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonDanScoreDataResponse> HandleNijiiro(GetDanScoreQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonDanScoreDataResponse> HandleGreen(GetDanScoreQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonDanScoreDataResponse> HandleBlue(GetDanScoreQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonDanScoreDataResponse> HandleYellow(GetDanScoreQuery request, CancellationToken cancellationToken);
}


