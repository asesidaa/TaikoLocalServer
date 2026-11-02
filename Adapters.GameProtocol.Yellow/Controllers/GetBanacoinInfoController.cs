namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/getbanacoininfo.php")]
public class GetBanacoinInfoController : BaseProtocolController<GetBanacoinInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetBanacoinInfo([FromBody] GetbanacoininfoRequest request)
    {
        Logger.LogInformation("Yellow GetBanacoinInfo request: {@Request}", request);
        return Ok(new GetbanacoininfoResponse { Result = 1 });
    }
}
