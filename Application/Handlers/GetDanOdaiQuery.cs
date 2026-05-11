using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetDanOdaiQuery(GameEra Era, uint[] DanIds, uint Type) : IRequest<List<DanData>>;

public partial class GetDanOdaiQueryHandler : IRequestHandler<GetDanOdaiQuery, List<DanData>>
{
    private readonly IGameDataCatalog gameDataService;

    public GetDanOdaiQueryHandler(IGameDataCatalog gameDataService)
    {
        this.gameDataService = gameDataService;
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
