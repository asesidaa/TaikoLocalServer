namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetTokenCountController : BaseProtocolController<GetTokenCountController>
{
    [HttpPost("v12r00_cn/chassis/gettokencount.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTokenCountCN00([FromBody] GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetTokenCountQuery((uint)request.Baid), HttpContext.RequestAborted);
        var response = TokenCountDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
