namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("Momoiro SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.Momoiro, request.Level.GetValueOrDefault(), request.ArySongNoes ?? []),
            HttpContext.RequestAborted);

        return Ok(SelfBestMappers.Map(common));
    }
}
