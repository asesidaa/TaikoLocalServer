namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/playresult.php")]
[Route("/v08r01/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Red PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Red, playResult),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
