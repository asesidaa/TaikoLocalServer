namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/getbanacoininfo.php")]
public class GetBanacoinInfoController : BaseProtocolController<GetBanacoinInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetBanacoinInfo([FromBody] GetbanacoininfoRequest request)
    {
        Logger.LogInformation("Blue GetBanacoinInfo request: {Request}", request.Stringify());
        return Ok(new GetbanacoininfoResponse { Result = 1 });
    }
}
