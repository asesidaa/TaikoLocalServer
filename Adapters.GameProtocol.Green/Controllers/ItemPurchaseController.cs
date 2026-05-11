namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/itempurchase.php")]
public class ItemPurchaseController : BaseProtocolController<ItemPurchaseController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ItemPurchase([FromBody] ItempurchaseRequest request)
    {
        Logger.LogInformation("Green ItemPurchase request: {Request}", request.Stringify());
        var common = await Mediator.Send(
            new ItemPurchaseCommand(request.Baid, request.ItemNo, request.ItemType, request.ItemId, request.ItemPrice),
            HttpContext.RequestAborted);
        return Ok(ItemShopMappers.Map(common));
    }
}
