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

        var commonResponse = await Mediator.Send(new GetAiScoreQuery(request.Baid, request.SongNo, request.Level));
        var response = AiScoreMappers.MapTo3906(commonResponse);

        return Ok(response);
    }

    [HttpPost("v12r00_cn/chassis/getaiscore.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiScore3209([FromBody] Models.v3209.GetAiScoreRequest request)
    {
        Logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var commonResponse =
            await Mediator.Send(new GetAiScoreQuery((uint)request.Baid, request.SongNo, request.Level));
        var response = AiScoreMappers.MapTo3209(commonResponse);

        return Ok(response);
    }
}