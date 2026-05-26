namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/crownsdata.php")]
public class CrownsDataController : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Blue CrownsData request: {Request}", request.Stringify());
        return Ok(new CrownsDataResponse { Result = 1 });
    }
}
