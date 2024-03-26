using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class InitialDataCheckController : BaseController<InitialDataCheckController>
{
    [HttpPost("/v12r08_ww/chassis/initialdatacheck_vaosv643.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Initial data check request: {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetInitialDataQuery());
        var response = InitialDataMappers.MapTo3906(commonResponse);

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/initialdatacheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheckCN([FromBody] Models.v3209.InitialdatacheckRequest request)
    {
        Logger.LogInformation("Initial data check request: {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetInitialDataQuery());
        var response = InitialDataMappers.MapTo3209(commonResponse);

        return Ok(response);
    }

}