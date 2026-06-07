namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost("/v12r00_cn/chassis/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBestCN00([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("SelfBestCN00 request : {@Request}", request);

        var commonResponse =
            await Mediator.Send(new GetSelfBestQuery((uint)request.Baid, GameEra.Nijiiro, request.Level, request.ArySongNoes), HttpContext.RequestAborted);
        var response = SelfBestMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
