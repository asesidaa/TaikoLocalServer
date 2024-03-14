using TaikoLocalServer.Handlers;
using TaikoLocalServer.Models.Application;
using AddTokenCountRequestMapper = TaikoLocalServer.Mappers.AddTokenCountRequestMapper;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class AddTokenCountController : BaseController<AddTokenCountController>
{
    [HttpPost("/v12r08_ww/chassis/addtokencount_7547j3o4.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> AddTokenCount([FromBody] AddTokenCountRequest request)
    {
        Logger.LogInformation("[3906] AddTokenCount request : {Request}", request.Stringify());

        var command = new AddTokenCountCommand(AddTokenCountRequestMapper.Map(request));
        await Mediator.Send(command);

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/addtokencount.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> AddTokenCount3209([FromBody] Models.v3209.AddTokenCountRequest request)
    {
        Logger.LogInformation("[3209] AddTokenCount request : {Request}", request.Stringify());

        var command = new AddTokenCountCommand(AddTokenCountRequestMapper.Map(request));
        await Mediator.Send(command);

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}