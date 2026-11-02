namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/itempurchase.php")]
public class ItemPurchaseController : BaseProtocolController<ItemPurchaseController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ItemPurchase([FromBody] ItempurchaseRequest request)
    {
        Logger.LogInformation("Yellow ItemPurchase request from {ChassisId}", request.ChassisId);
        var common = await Mediator.Send(ItemShopMappers.Map(request), HttpContext.RequestAborted);
        return Ok(ItemShopMappers.Map(common));
    }
}
