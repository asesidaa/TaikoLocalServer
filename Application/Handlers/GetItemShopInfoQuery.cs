namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetItemShopInfoQuery : IRequest<CommonItemShopInfoResponse>;

public partial class GetItemShopInfoQueryHandler(
    IGameDataCatalog gameDataService,
    ILogger<GetItemShopInfoQueryHandler> logger)
    : IRequestHandler<GetItemShopInfoQuery, CommonItemShopInfoResponse>
{
    public partial ValueTask<CommonItemShopInfoResponse> Handle(GetItemShopInfoQuery request, CancellationToken cancellationToken);
}
