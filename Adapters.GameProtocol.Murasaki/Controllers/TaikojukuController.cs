namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost(MurasakiRoutePrefixes.Final + "/taikojuku.php")]
    [Produces("application/protobuf")]
    public Task<IActionResult> FinalTaikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("Murasaki final Taikojuku request: {@Request}", request);
        return HandleTaikojuku(request);
    }

    [HttpPost(MurasakiRoutePrefixes.Compatibility + "/taikojuku.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("Murasaki compatibility Taikojuku request: {@Request}", request);
        return await HandleTaikojuku(request);
    }

    private async Task<IActionResult> HandleTaikojuku(TaikojukuRequest request)
    {
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.Murasaki, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(TaikojukuMappers.Map(common));
    }
}
