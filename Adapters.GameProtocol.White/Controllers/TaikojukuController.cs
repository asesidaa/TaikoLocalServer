using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/taikojuku.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("White Taikojuku request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.White, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(TaikojukuMappers.Map(common));
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/taikojuku.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyTaikojuku([FromBody] LegacyWire.TaikojukuRequest request)
    {
        Logger.LogInformation("White legacy Taikojuku request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.White, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(LegacyWire.LegacyTaikojukuMappers.Map(common));
    }
}
