namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/headclerk2.php")]
public class HeadClerk2Controller : BaseProtocolController<HeadClerk2Controller>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HeadClerk2([FromBody] HeadClerk2Request request)
    {
        Logger.LogInformation("Blue HeadClerk2 request: {@Request}", request);
        return Ok(new HeadClerk2Response { Result = 1 });
    }
}
