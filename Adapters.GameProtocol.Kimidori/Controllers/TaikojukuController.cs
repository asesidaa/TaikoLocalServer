namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/taikojuku.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Taikojuku([FromBody] FinalWire.TaikojukuRequest request)
    {
        Logger.LogInformation("Kimidori final Taikojuku request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.Kimidori, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(FinalTaikojukuMappers.Map(common));
    }
}
