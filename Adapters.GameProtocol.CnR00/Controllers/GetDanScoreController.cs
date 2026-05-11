namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetDanScoreController : BaseProtocolController<GetDanScoreController>
{
    [HttpPost("/v12r00_cn/chassis/getdanscore.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanScoreCN00([FromBody] GetDanScoreRequest request)
    {
        Logger.LogInformation("GetDanScoreCN00 request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetDanScoreQuery((uint)request.Baid, GameEra.Nijiiro, request.Type, request.DanIds), HttpContext.RequestAborted);
        var response = DanScoreMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
