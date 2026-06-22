namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost(MurasakiRoutePrefixes.Final + "/playresult.php")]
    [Produces("application/protobuf")]
    public Task<IActionResult> FinalPlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Murasaki final PlayResult request: {@Request}", request);
        return request.PlayMode == (uint)PlayMode.DanMode
            ? Task.FromResult<IActionResult>(Ok(new PlayResultResponse { Result = 1 }))
            : HandlePlayResult(request);
    }

    [HttpPost(MurasakiRoutePrefixes.Compatibility + "/playresult.php")]
    [Produces("application/protobuf")]
    public Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Murasaki compatibility PlayResult request: {@Request}", request);
        return HandlePlayResult(request);
    }

    private async Task<IActionResult> HandlePlayResult(PlayResultRequest request)
    {
        var playResult = PlayResultMappers.Map(request);
        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.Murasaki, playResult),
            HttpContext.RequestAborted);
        return Ok(PlayResultMappers.Map(result));
    }
}
