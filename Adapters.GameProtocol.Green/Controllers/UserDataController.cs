namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Green UserData request: {Request}", request.Stringify());
        var common = await Mediator.Send(new UserDataQuery(request.Baid, GameEra.Green), HttpContext.RequestAborted);
        return Ok(UserDataMappers.Map(common));
    }
}
