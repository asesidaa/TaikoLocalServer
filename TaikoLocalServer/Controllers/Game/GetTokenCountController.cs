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

        var commonResponse = await Mediator.Send(new GetTokenCountQuery(request.Baid));
        var response = TokenCountDataMappers.MapTo3906(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("v12r00_cn/chassis/gettokencount.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTokenCount3209([FromBody] Models.v3209.GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetTokenCountQuery((uint)request.Baid));
        var response = TokenCountDataMappers.MapTo3209(commonResponse);

        return Ok(response);
    }
}