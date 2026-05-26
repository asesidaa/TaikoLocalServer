namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/battleuserdata.php")]
public class BattleUserDataController : BaseProtocolController<BattleUserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BattleUserData([FromBody] BattleUserDataRequest request)
    {
        Logger.LogInformation("Blue BattleUserData request: {Request}", request.Stringify());
        return Ok(new BattleUserDataResponse { Result = 1 });
    }
}
