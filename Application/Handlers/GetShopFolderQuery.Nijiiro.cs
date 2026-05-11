namespace TaikoLocalServer.Application.Handlers;

public partial class GetShopFolderHandler
{
    private partial ValueTask<CommonGetShopFolderResponse> HandleNijiiro(GetShopFolderQuery request, CancellationToken cancellationToken)
    {
        gameDataService.Nijiiro().GetTokenDataDictionary().TryGetValue("seasonTokenId", out var seasonTokenId);

        var shopFolderList = gameDataService.Nijiiro().GetShopFolderList();

        var response = new CommonGetShopFolderResponse
        {
            Result = 1,
            TokenId = seasonTokenId > 0 ? (uint)seasonTokenId : 1,
            VerupNo = gameDataService.Nijiiro().GetShopFolderVerup() + (seasonTokenId > 0 ? (uint)seasonTokenId : 1),
            AryShopFolderDatas = shopFolderList
        };

        return ValueTask.FromResult(response);
    }
}
