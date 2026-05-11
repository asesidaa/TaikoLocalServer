namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetShopFolderQuery(GameEra Era) : IRequest<CommonGetShopFolderResponse>;

public class GetShopFolderHandler(IGameDataCatalog gameDataService)
    : IRequestHandler<GetShopFolderQuery, CommonGetShopFolderResponse>
{
    public ValueTask<CommonGetShopFolderResponse> Handle(GetShopFolderQuery request, CancellationToken cancellationToken)
    {
        gameDataService.GetTokenDataDictionary().TryGetValue("seasonTokenId", out var seasonTokenId);

        var shopFolderList = gameDataService.GetShopFolderList();

        var response = new CommonGetShopFolderResponse
        {
            Result = 1,
            TokenId = seasonTokenId > 0 ? (uint)seasonTokenId : 1,
            VerupNo = gameDataService.GetShopFolderVerup() + (seasonTokenId > 0 ? (uint)seasonTokenId : 1),
            AryShopFolderDatas = shopFolderList
        };

        return ValueTask.FromResult(response);
    }
}
