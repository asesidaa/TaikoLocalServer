namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Red PlayResult request: {@Request}", request);
        var common = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdatePlayResultCommand(request.Baid, GameEra.Red, common),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
