using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/gettelop.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("White GetTelop request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.White, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(GetTelopMappers.Map(common));
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/gettelop.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyGetTelop([FromBody] LegacyWire.GettelopRequest request)
    {
        Logger.LogInformation("White legacy GetTelop request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.White, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(LegacyWire.LegacyGetTelopMappers.Map(common));
    }
}
