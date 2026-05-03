using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetAiScoreController : BaseController<GetAiScoreController>
{
    [HttpPost("/v12r08_ww/chassis/getaiscore_lp38po4w.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiScore([FromBody] GetAiScoreRequest request)
    {
        Logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetAiScoreQuery(request.Baid, request.SongNo, request.Level), HttpContext.RequestAborted);
        var response = AiScoreMappers.MapToWW08(commonResponse);

        return Ok(response);
    }

    [HttpPost("v12r00_cn/chassis/getaiscore.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiScoreCN00([FromBody] Models.CN00.GetAiScoreRequest request)
    {
        Logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var commonResponse =
            await Mediator.Send(new GetAiScoreQuery((uint)request.Baid, request.SongNo, request.Level), HttpContext.RequestAborted);
        var response = AiScoreMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}