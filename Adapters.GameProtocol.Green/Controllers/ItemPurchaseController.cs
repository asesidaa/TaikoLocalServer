namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/itempurchase.php")]
public class ItemPurchaseController : BaseProtocolController<ItemPurchaseController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ItemPurchase([FromBody] ItempurchaseRequest request)
    {
        Logger.LogInformation("Green ItemPurchase request: {Request}", request.Stringify());
        return Ok(new ItempurchaseResponse { Result = 1 });
    }
}
