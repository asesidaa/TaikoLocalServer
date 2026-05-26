namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/banacoinerrorlog.php")]
public class BanacoinErrorLogController : BaseProtocolController<BanacoinErrorLogController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinErrorLog([FromBody] BanacoinerrorlogRequest request)
    {
        Logger.LogInformation("Blue BanacoinErrorLog request: {Request}", request.Stringify());
        return Ok(new BanacoinerrorlogResponse { Result = 1 });
    }
}
