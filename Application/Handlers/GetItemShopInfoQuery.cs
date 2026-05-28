namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetItemShopInfoQuery(GameEra Era) : IRequest<CommonItemShopInfoResponse>;

public partial class GetItemShopInfoQueryHandler(
    IGameDataCatalog gameDataService,
    ILogger<GetItemShopInfoQueryHandler> logger)
    : IRequestHandler<GetItemShopInfoQuery, CommonItemShopInfoResponse>
{
    public ValueTask<CommonItemShopInfoResponse> Handle(GetItemShopInfoQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonItemShopInfoResponse> HandleGreen(GetItemShopInfoQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonItemShopInfoResponse> HandleBlue(GetItemShopInfoQuery request, CancellationToken cancellationToken);
}
