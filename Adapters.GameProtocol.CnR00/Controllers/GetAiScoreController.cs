namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetAiScoreController : BaseProtocolController<GetAiScoreController>
{
    [HttpPost("v12r00_cn/chassis/getaiscore.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiScoreCN00([FromBody] GetAiScoreRequest request)
    {
        Logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var commonResponse =
            await Mediator.Send(new GetAiScoreQuery((uint)request.Baid, GameEra.Nijiiro, request.SongNo, request.Level), HttpContext.RequestAborted);
        var response = AiScoreMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
