namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost(MurasakiRoutePrefixes.Game + "/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Murasaki PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);
        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Murasaki, playResult),
            HttpContext.RequestAborted);
        return Ok(PlayResultMappers.Map(result));
    }
}
