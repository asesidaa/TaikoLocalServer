namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetShopFolderController : BaseProtocolController<GetShopFolderController>
{
    [HttpPost("/v12r00_cn/chassis/getshopfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetShopFolderCN00([FromBody] GetShopFolderRequest request)
    {
        Logger.LogInformation("GetShopFolder request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetShopFolderQuery(), HttpContext.RequestAborted);
        var response = ShopFolderDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
