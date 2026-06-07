namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Blue PlayResult request: {@Request}", request);
        var common = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
