namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/headclerk2.php")]
public class HeadClerk2Controller : BaseProtocolController<HeadClerk2Controller>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HeadClerk2([FromBody] HeadClerk2Request request)
    {
        Logger.LogInformation("Yellow HeadClerk2 request from {ChassisId}", request.ChassisId);
        return Ok(new HeadClerk2Response { Result = 1 });
    }
}
