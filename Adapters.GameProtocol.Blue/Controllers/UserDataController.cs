namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Blue UserData request: {Request}", request.Stringify());
        return Ok(new UserDataResponse { Result = 1 });
    }
}
