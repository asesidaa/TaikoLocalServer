namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost(MurasakiRoutePrefixes.Final + "/gettelop.php")]
    [HttpPost(MurasakiRoutePrefixes.Compatibility + "/gettelop.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("Murasaki GetTelop request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.Murasaki, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(GetTelopMappers.Map(common));
    }
}
