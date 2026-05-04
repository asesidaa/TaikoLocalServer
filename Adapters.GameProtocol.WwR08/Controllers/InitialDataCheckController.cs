namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost("/v12r08_ww/chassis/initialdatacheck_vaosv643.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Initial data check request: {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetInitialDataQuery(), HttpContext.RequestAborted);
        var response = InitialDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
