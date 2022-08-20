using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getshopfolder.php")]
[ApiController]
public class GetShopFolderController : ControllerBase
{
    private readonly ILogger<GetShopFolderController> logger;
    public GetShopFolderController(ILogger<GetShopFolderController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetShopFolder([FromBody] GetShopFolderRequest request)
    {
        logger.LogInformation("GetShopFolder request : {Request}", request.Stringify());

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