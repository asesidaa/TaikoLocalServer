namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetShopFolderQuery(GameEra Era) : IRequest<CommonGetShopFolderResponse>;

public partial class GetShopFolderHandler(IGameDataCatalog gameDataService)
    : IRequestHandler<GetShopFolderQuery, CommonGetShopFolderResponse>
{
    public ValueTask<CommonGetShopFolderResponse> Handle(GetShopFolderQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonGetShopFolderResponse> HandleNijiiro(GetShopFolderQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetShopFolderResponse> HandleGreen(GetShopFolderQuery request, CancellationToken cancellationToken);
}
