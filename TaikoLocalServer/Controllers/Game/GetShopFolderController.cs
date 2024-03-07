namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r08_ww/chassis/getshopfolder_w4xik0uw.php")]
[ApiController]
public class GetShopFolderController : BaseController<GetShopFolderController>
{
    private readonly IGameDataService gameDataService;

    public GetShopFolderController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetShopFolder([FromBody] GetShopFolderRequest request)
    {
        Logger.LogInformation("GetShopFolder request : {Request}", request.Stringify());

        gameDataService.GetTokenDataDictionary().TryGetValue("shopTokenId", out var shopTokenId);

        var shopFolderList = gameDataService.GetShopFolderList();

        var response = new GetShopFolderResponse
        {
            Result = 1,
            TokenId = shopTokenId > 0 ? (uint)shopTokenId : 1,
            VerupNo = 2
        };

        response.AryShopFolderDatas.AddRange(shopFolderList);

        return Ok(response);
    }
}