namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class SongPurchaseController : BaseProtocolController<SongPurchaseController>
{
    [HttpPost("/v12r08_ww/chassis/songpurchase_wm2fh5bl.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SongPurchase([FromBody] SongPurchaseRequest request)
    {
        Logger.LogInformation("SongPurchase request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(SongPurchaseMappers.MapToCommand(request), HttpContext.RequestAborted);
        var response = SongPurchaseMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
