using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetTokenCountController : BaseController<GetTokenCountController>
{
    [HttpPost("/v12r08_ww/chassis/gettokencount_iut9g23g.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTokenCount([FromBody] GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetTokenCountQuery(request.Baid), HttpContext.RequestAborted);
        var response = TokenCountDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("v12r00_cn/chassis/gettokencount.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTokenCountCN00([FromBody] Models.CN00.GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetTokenCountQuery((uint)request.Baid), HttpContext.RequestAborted);
        var response = TokenCountDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}