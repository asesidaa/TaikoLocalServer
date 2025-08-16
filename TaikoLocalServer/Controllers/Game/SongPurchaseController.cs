using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class SongPurchaseController : BaseController<SongPurchaseController>
{
    [HttpPost("/v12r08_ww/chassis/songpurchase_wm2fh5bl.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SongPurchase([FromBody] SongPurchaseRequest request)
    {
        Logger.LogInformation("SongPurchase request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(SongPurchaseMappers.MapToCommand(request));
        var response = SongPurchaseMappers.MapToWW08(commonResponse);
       
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/songpurchase.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SongPurchaseCN00([FromBody] Models.CN00.SongPurchaseRequest request)
    {
        Logger.LogInformation("SongPurchase request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(SongPurchaseMappers.MapToCommand(request));
        var response = SongPurchaseMappers.MapToCN00(commonResponse);
        return Ok(response);
    }
}