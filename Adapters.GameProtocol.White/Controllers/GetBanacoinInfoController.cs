namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/getbanacoininfo.php")]
[Route(WhiteRoutePrefixes.Compatibility + "/getbanacoininfo.php")]
public class GetBanacoinInfoController : BaseProtocolController<GetBanacoinInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetBanacoinInfo([FromBody] GetbanacoininfoRequest request)
    {
        Logger.LogInformation("White GetBanacoinInfo request: {@Request}", request);
        return Ok(new GetbanacoininfoResponse { Result = 1 });
    }
}
