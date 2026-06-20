namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/taikojuku.php")]
[Route(WhiteRoutePrefixes.Compatibility + "/taikojuku.php")]
public sealed class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("White Taikojuku request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.White, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(TaikojukuMappers.Map(common));
    }
}
