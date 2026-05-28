namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/itempurchase.php")]
public class ItemPurchaseController : BaseProtocolController<ItemPurchaseController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ItemPurchase([FromBody] ItempurchaseRequest request)
    {
        Logger.LogInformation("Blue ItemPurchase request: {Request}", request.Stringify());
        var common = await Mediator.Send(ItemShopMappers.Map(request), HttpContext.RequestAborted);
        return Ok(ItemShopMappers.Map(common));
    }
}
