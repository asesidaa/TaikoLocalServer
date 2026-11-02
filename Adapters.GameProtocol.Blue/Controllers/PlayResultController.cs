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
        var ac15 = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Blue, ac15),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
