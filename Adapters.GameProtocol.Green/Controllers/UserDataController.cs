namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Green UserData request: {Request}", request.Stringify());
        return Ok(new UserDataResponse
        {
            Result = 1,
            IsExplain = false,
            IsDevil = false,
            IsChallengecompe = false,
            IsTojiru = false
        });
    }
}
