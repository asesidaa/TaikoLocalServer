using GameDatabase.Entities;
using TaikoLocalServer.Handlers;
using TaikoLocalServer.Mappers;
using Throw;

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
        var response = DanScoreMappers.MapTo3906(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getdanscore.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanScore3209([FromBody] Models.v3209.GetDanScoreRequest request)
    {
        Logger.LogInformation("GetDanScore3209 request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetDanScoreQuery((uint)request.Baid, request.Type, request.DanIds));
        var response = DanScoreMappers.MapTo3209(commonResponse);

        return Ok(response);
    }
}