namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/songpurchase.php")]
[ApiController]
public class SongPurchaseController : BaseController<SongPurchaseController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SongPurchase([FromBody] SongPurchaseRequest request)
    {
        Logger.LogInformation("SongPurchase request : {Request}", request.Stringify());

        var response = new SongPurchaseResponse
        {
            Result = 1,
            TokenCount = (int)(10 - request.Price)
        };

        return Ok(response);
    }
}