namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class AddTokenCountController : BaseProtocolController<AddTokenCountController>
{
    [HttpPost("/v12r08_ww/chassis/addtokencount_7547j3o4.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> AddTokenCount([FromBody] AddTokenCountRequest request)
    {
        Logger.LogInformation("[WW08] AddTokenCount request : {Request}", request.Stringify());

        var command = new AddTokenCountCommand(AddTokenCountRequestMapper.Map(request));
        await Mediator.Send(command, HttpContext.RequestAborted);

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}
