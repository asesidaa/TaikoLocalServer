using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/songpurchase.php")]
[ApiController]
public class SongPurchaseController : ControllerBase
{
    private readonly ILogger<SongPurchaseController> logger;
    public SongPurchaseController(ILogger<SongPurchaseController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SongPurchase([FromBody] SongPurchaseRequest request)
    {
        logger.LogInformation("SongPurchase request : {Request}", request.Stringify());

        var response = new SongPurchaseResponse
        {
            Result = 1,
            TokenCount = (int)(10 - request.Price)
        };

        return Ok(response);
    }
}