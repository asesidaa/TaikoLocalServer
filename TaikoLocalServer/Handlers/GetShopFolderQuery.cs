namespace TaikoLocalServer.Handlers;

public record GetShopFolderQuery : IRequest<CommonGetShopFolderResponse>;

public class GetShopFolderHandler(IGameDataService gameDataService)
    : IRequestHandler<GetShopFolderQuery, CommonGetShopFolderResponse>
{
    public Task<CommonGetShopFolderResponse> Handle(GetShopFolderQuery request, CancellationToken cancellationToken)
    {
        gameDataService.GetTokenDataDictionary().TryGetValue("seasonTokenId", out var seasonTokenId);

        var shopFolderList = gameDataService.GetShopFolderList();

        var response = new CommonGetShopFolderResponse
        {
            Result = 1,
            TokenId = seasonTokenId > 0 ? (uint)seasonTokenId : 1,
            VerupNo = 2,
            AryShopFolderDatas = shopFolderList
        };

        return Task.FromResult(response);
    }
}