namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Blue PlayResult request: {Request}", request.Stringify());
        return Ok(new PlayResultResponse { Result = 1 });
    }
}
