namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class SongPurchaseController : BaseProtocolController<SongPurchaseController>
{
    [HttpPost("/v12r00_cn/chassis/songpurchase.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SongPurchaseCN00([FromBody] SongPurchaseRequest request)
    {
        Logger.LogInformation("SongPurchase request : {@Request}", request);

        var commonResponse = await Mediator.Send(SongPurchaseMappers.MapToCommand(request), HttpContext.RequestAborted);
        var response = SongPurchaseMappers.MapToCN00(commonResponse);
        return Ok(response);
    }
}
