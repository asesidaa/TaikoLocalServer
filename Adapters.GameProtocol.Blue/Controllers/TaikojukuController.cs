namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/taikojuku.php")]
public class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("Blue Taikojuku request: {Request}", request.Stringify());
        return Ok(new TaikojukuResponse { Result = 1 });
    }
}
