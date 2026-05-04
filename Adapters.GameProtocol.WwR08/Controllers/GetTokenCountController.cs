namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetTokenCountController : BaseProtocolController<GetTokenCountController>
{
    [HttpPost("/v12r08_ww/chassis/gettokencount_iut9g23g.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTokenCount([FromBody] GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetTokenCountQuery(request.Baid), HttpContext.RequestAborted);
        var response = TokenCountDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
