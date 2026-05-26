namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/itempurchase.php")]
public class ItemPurchaseController : BaseProtocolController<ItemPurchaseController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ItemPurchase([FromBody] ItempurchaseRequest request)
    {
        Logger.LogInformation("Blue ItemPurchase request: {Request}", request.Stringify());
        return Ok(new ItempurchaseResponse { Result = 1 });
    }
}
