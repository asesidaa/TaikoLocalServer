namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Red route probe baidcheck.php request: {@Request}", request);
        return Ok(new BAIDResponse { Result = 1 });
    }
}
