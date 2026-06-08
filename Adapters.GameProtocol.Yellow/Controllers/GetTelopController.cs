namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/gettelop.php")]
public class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("Yellow GetTelop request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.Yellow, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(GetTelopMappers.Map(common));
    }
}
