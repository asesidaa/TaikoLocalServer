namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetAiDataController : BaseProtocolController<GetAiDataController>
{
    [HttpPost("/v12r00_cn/chassis/getaidata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiDataCN00([FromBody] GetAiDataRequest request)
    {
        Logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetAiDataQuery((uint)request.Baid, GameEra.Nijiiro), HttpContext.RequestAborted);
        var response = AiDataResponseMapper.MapToCN00(commonResponse);
        return Ok(response);
    }
}
