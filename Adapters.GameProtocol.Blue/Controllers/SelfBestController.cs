namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/selfbest.php")]
public class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("Blue SelfBest request: {Request}", request.Stringify());
        return Ok(new SelfBestResponse { Result = 1 });
    }
}
