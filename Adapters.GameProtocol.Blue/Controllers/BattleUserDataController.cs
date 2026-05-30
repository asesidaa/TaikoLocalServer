namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/battleuserdata.php")]
public class BattleUserDataController : BaseProtocolController<BattleUserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> BattleUserData([FromBody] BattleUserDataRequest request)
    {
        Logger.LogInformation("Blue BattleUserData request: {Request}", request.Stringify());
        var common = await Mediator.Send(new GetBattleUserDataQuery(request.Baid), HttpContext.RequestAborted);
        return Ok(BattleUserDataMappers.Map(common));
    }
}
