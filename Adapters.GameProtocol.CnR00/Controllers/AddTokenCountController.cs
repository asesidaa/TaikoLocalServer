namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class AddTokenCountController : BaseProtocolController<AddTokenCountController>
{
    [HttpPost("/v12r00_cn/chassis/addtokencount.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> AddTokenCountCN00([FromBody] AddTokenCountRequest request)
    {
        Logger.LogInformation("[CN00] AddTokenCount request : {Request}", request.Stringify());

        var command = new AddTokenCountCommand(GameEra.Nijiiro, AddTokenCountRequestMapper.Map(request));
        await Mediator.Send(command, HttpContext.RequestAborted);

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}
