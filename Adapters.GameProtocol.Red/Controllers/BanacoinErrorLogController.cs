namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00/chassis/banacoinerrorlog.php")]
[Route("/v08r01/chassis/banacoinerrorlog.php")]
public class BanacoinErrorLogController : BaseProtocolController<BanacoinErrorLogController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinErrorLog([FromBody] BanacoinerrorlogRequest request)
    {
        Logger.LogInformation("Red route probe banacoinerrorlog.php request: {@Request}", request);
        return Ok(new BanacoinerrorlogResponse { Result = 1 });
    }
}
