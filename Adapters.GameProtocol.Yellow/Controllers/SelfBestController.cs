namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/selfbest.php")]
public class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("Yellow SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.Yellow, request.Level.GetValueOrDefault(), request.ArySongNoes ?? []),
            HttpContext.RequestAborted);
        return Ok(SelfBestMappers.Map(common));
    }
}
