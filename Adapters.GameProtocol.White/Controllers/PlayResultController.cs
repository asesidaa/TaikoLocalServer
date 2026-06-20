namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/playresult.php")]
[Route(WhiteRoutePrefixes.Compatibility + "/playresult.php")]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("White PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.White, playResult),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
