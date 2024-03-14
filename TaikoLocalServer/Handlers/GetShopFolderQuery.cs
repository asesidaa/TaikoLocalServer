using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Handlers;

public record GetShopFolderQuery : IRequest<CommonGetShopFolderResponse>;

public class GetShopFolderHandler(IGameDataService gameDataService)
    : IRequestHandler<GetShopFolderQuery, CommonGetShopFolderResponse>
{
    public Task<CommonGetShopFolderResponse> Handle(GetShopFolderQuery request, CancellationToken cancellationToken)
    {
        gameDataService.GetTokenDataDictionary().TryGetValue("shopTokenId", out var shopTokenId);

        var shopFolderList = gameDataService.GetShopFolderList();
        
        var response = new CommonGetShopFolderResponse
        {
            Result = 1,
            TokenId = shopTokenId > 0 ? (uint)shopTokenId : 1,
            VerupNo = 2,
            AryShopFolderDatas = shopFolderList
        };
        
        return Task.FromResult(response);
    }
}