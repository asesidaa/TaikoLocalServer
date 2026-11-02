namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/headclerk2.php")]
[Route("/v08r01/chassis/headclerk2.php")]
public class HeadClerk2Controller : BaseProtocolController<HeadClerk2Controller>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HeadClerk2([FromBody] HeadClerk2Request request)
    {
        Logger.LogInformation("Red route probe headclerk2.php request: {@Request}", request);
        return Ok(new HeadClerk2Response { Result = 1 });
    }
}
