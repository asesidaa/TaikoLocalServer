using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct BaidQuery(GameEra Era, string AccessCode) : IRequest<CommonBaidResponse>;

public partial class BaidQueryHandler(
    ITaikoDbContext context,
    ILogger<BaidQueryHandler> logger,
    IGameDataCatalog gameDataService)
    : IRequestHandler<BaidQuery, CommonBaidResponse>
{
    public ValueTask<CommonBaidResponse> Handle(BaidQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonBaidResponse> HandleNijiiro(BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonBaidResponse> HandleGreen(BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonBaidResponse> HandleBlue(BaidQuery request, CancellationToken cancellationToken);
}
