using GameDatabase.Entities;
using TaikoLocalServer.Handlers;
using TaikoLocalServer.Mappers;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class SelfBestController : BaseController<SelfBestController>
{
    [HttpPost("/v12r08_ww/chassis/selfbest_5nz47auu.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("SelfBest request : {Request}", request.Stringify());

        var commonResponse =
            await Mediator.Send(new GetSelfBestQuery(request.Baid, request.Level, request.ArySongNoes));
        var response = SelfBestMappers.MapTo3906(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest3209([FromBody] Models.v3209.SelfBestRequest request)
    {
        Logger.LogInformation("SelfBest3209 request : {Request}", request.Stringify());

        var commonResponse =
            await Mediator.Send(new GetSelfBestQuery((uint)request.Baid, request.Level, request.ArySongNoes));
        var response = SelfBestMappers.MapTo3209(commonResponse);

        return Ok(response);
    }
}