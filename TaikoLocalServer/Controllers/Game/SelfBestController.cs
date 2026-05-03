using TaikoLocalServer.Mappers;

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
            await Mediator.Send(new GetSelfBestQuery(request.Baid, request.Level, request.ArySongNoes), HttpContext.RequestAborted);
        var response = SelfBestMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/selfbest.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBestCN00([FromBody] Models.CN00.SelfBestRequest request)
    {
        Logger.LogInformation("SelfBestCN00 request : {Request}", request.Stringify());

        var commonResponse =
            await Mediator.Send(new GetSelfBestQuery((uint)request.Baid, request.Level, request.ArySongNoes), HttpContext.RequestAborted);
        var response = SelfBestMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}