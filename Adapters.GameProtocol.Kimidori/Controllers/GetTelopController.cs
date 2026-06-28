namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/gettelop.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalGetTelop([FromBody] FinalWire.GettelopRequest request)
    {
        Logger.LogInformation("Kimidori final GetTelop request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.Kimidori, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(FinalGetTelopMappers.Map(common));
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/gettelop.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("Kimidori GetTelop request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.Kimidori, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(GetTelopMappers.Map(common));
    }
}
