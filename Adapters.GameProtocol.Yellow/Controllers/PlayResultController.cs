namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Yellow PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Yellow, playResult),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
