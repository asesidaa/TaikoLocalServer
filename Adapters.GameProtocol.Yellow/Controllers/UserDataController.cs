namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Yellow UserData request: {@Request}", request);
        var common = await Mediator.Send(new UserDataQuery(request.Baid, GameEra.Yellow), HttpContext.RequestAborted);
        return Ok(UserDataMappers.Map(common));
    }
}
