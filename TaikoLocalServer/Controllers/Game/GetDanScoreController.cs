using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetDanScoreController : BaseController<GetDanScoreController>
{
    [HttpPost("/v12r08_ww/chassis/getdanscore_frqhg7q6.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanScore([FromBody] GetDanScoreRequest request)
    {
        Logger.LogInformation("GetDanScore request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetDanScoreQuery(request.Baid, request.Type, request.DanIds));
        var response = DanScoreMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getdanscore.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanScoreCN00([FromBody] Models.CN00.GetDanScoreRequest request)
    {
        Logger.LogInformation("GetDanScoreCN00 request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetDanScoreQuery((uint)request.Baid, request.Type, request.DanIds));
        var response = DanScoreMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}