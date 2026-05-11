
namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetTokenCountQuery(uint Baid, GameEra Era) : IRequest<CommonGetTokenCountResponse>;

public partial class GetTokenCountQueryHandler(IGameDataCatalog gameDataService,
    ITaikoDbContext context,
#pragma warning disable CS9113 // Parameter is unread.
    ILogger<GetTokenCountQueryHandler> logger)
#pragma warning restore CS9113 // Parameter is unread.
    : IRequestHandler<GetTokenCountQuery, CommonGetTokenCountResponse>
{
    public ValueTask<CommonGetTokenCountResponse> Handle(GetTokenCountQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonGetTokenCountResponse> HandleNijiiro(GetTokenCountQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetTokenCountResponse> HandleGreen(GetTokenCountQuery request, CancellationToken cancellationToken);
}
