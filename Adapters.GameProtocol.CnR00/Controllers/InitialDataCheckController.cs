namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost("/v12r00_cn/chassis/initialdatacheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheckCN([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Initial data check request: {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetInitialDataQuery(GameEra.Nijiiro), HttpContext.RequestAborted);
        var response = InitialDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
