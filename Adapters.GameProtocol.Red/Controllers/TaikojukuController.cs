namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00/chassis/taikojuku.php")]
[Route("/v08r01/chassis/taikojuku.php")]
public class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("Red Taikojuku request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.Red, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(TaikojukuMappers.Map(common));
    }
}
