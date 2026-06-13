namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/userdata.php")]
[Route("/v08r01/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Red UserData request: {@Request}", request);
        var common = await Mediator.Send(new UserDataQuery(request.Baid, GameEra.Red), HttpContext.RequestAborted);
        return Ok(UserDataMappers.Map(common));
    }
}
