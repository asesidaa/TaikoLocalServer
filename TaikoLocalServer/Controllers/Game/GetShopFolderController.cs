namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getshopfolder.php")]
[ApiController]
public class GetShopFolderController : BaseController<GetShopFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetShopFolder([FromBody] GetShopFolderRequest request)
    {
        Logger.LogInformation("GetShopFolder request : {Request}", request.Stringify());

        var response = new GetShopFolderResponse
        {
            Result = 1,
            TokenId = 1,
            VerupNo = 2
        };

        response.AryShopFolderDatas.Add(new GetShopFolderResponse.ShopFolderData
        {
            Price = 1,
            SongNo = 2
        });

        return Ok(response);
    }
}