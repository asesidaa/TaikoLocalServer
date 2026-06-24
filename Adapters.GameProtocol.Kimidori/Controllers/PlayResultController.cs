namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Kimidori PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);
        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Kimidori, playResult),
            HttpContext.RequestAborted);
        return Ok(PlayResultMappers.Map(result));
    }
}
