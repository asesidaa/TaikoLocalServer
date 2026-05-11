namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/getghostscore.php")]
public class GetGhostScoreController : BaseProtocolController<GetGhostScoreController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetGhostScore([FromBody] GetghostscoreRequest request)
    {
        Logger.LogInformation("Green GetGhostScore request: {Request}", request.Stringify());
        var common = await Mediator.Send(
            new GetGhostScoreQuery(request.Baid, request.SongNo, request.Level),
            HttpContext.RequestAborted);
        return Ok(GhostMappers.Map(common));
    }
}
