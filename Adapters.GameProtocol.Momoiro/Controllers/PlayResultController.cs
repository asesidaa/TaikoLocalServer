namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Momoiro PlayResult request: {@Request}", request);
        var playResult = PlayResultMappers.Map(request);
        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Momoiro, playResult),
            HttpContext.RequestAborted);
        return Ok(PlayResultMappers.Map(result));
    }
}
