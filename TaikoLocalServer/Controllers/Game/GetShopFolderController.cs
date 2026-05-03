using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetShopFolderController : BaseController<GetShopFolderController>
{
    [HttpPost("/v12r08_ww/chassis/getshopfolder_w4xik0uw.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetShopFolder([FromBody] GetShopFolderRequest request)
    {
        Logger.LogInformation("GetShopFolder request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetShopFolderQuery(), HttpContext.RequestAborted);
        var response = ShopFolderDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getshopfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetShopFolderCN00([FromBody] Models.CN00.GetShopFolderRequest request)
    {
        Logger.LogInformation("GetShopFolder request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetShopFolderQuery(), HttpContext.RequestAborted);
        var response = ShopFolderDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}