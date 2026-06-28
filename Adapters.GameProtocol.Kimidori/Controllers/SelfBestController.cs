namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalSelfBest([FromBody] FinalWire.SelfBestRequest request)
    {
        Logger.LogInformation("Kimidori final SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.Kimidori, request.Level ?? 0, request.ArySongNoes ?? []),
            HttpContext.RequestAborted);
        return Ok(FinalSelfBestMappers.Map(common));
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("Kimidori SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.Kimidori, request.Level.GetValueOrDefault(), request.ArySongNoes ?? []),
            HttpContext.RequestAborted);
        return Ok(SelfBestMappers.Map(common));
    }
}
