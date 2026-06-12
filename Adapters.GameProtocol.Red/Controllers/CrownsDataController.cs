namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/crownsdata.php")]
public class CrownsDataController : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Red route probe crownsdata.php request: {@Request}", request);
        return Ok(new CrownsDataResponse { Result = 1 });
    }
}
