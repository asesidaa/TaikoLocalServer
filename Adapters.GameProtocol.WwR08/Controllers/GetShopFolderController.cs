namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetShopFolderController : BaseProtocolController<GetShopFolderController>
{
    [HttpPost("/v12r08_ww/chassis/getshopfolder_w4xik0uw.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetShopFolder([FromBody] GetShopFolderRequest request)
    {
        Logger.LogInformation("GetShopFolder request : {@Request}", request);

        var commonResponse = await Mediator.Send(new GetShopFolderQuery(GameEra.Nijiiro), HttpContext.RequestAborted);
        var response = ShopFolderDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
