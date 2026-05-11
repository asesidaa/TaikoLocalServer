namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Green Baid request: {Request}", request.Stringify());
        var common = await Mediator.Send(new BaidQuery(GameEra.Green, request.AccessCode), HttpContext.RequestAborted);

        var response = BaidResponseMapper.Map(common);
        response.AccessCode = request.AccessCode;
        response.IsPublish = true;
        response.PlayerType = common.IsNewUser ? 1u : 0u;

        return Ok(response);
    }
}
