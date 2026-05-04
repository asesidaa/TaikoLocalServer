namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetAiDataController : BaseProtocolController<GetAiDataController>
{
    [HttpPost("/v12r08_ww/chassis/getaidata_6x30b9nr.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiData([FromBody] GetAiDataRequest request)
    {
        Logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetAiDataQuery(request.Baid), HttpContext.RequestAborted);
        var response = AiDataResponseMapper.MapToWW08(commonResponse);
        return Ok(response);
    }
}
