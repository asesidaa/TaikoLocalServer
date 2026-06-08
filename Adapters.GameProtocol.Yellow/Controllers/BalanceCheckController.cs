namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/balancecheck.php")]
public class BalanceCheckController : BaseProtocolController<BalanceCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BalanceCheck([FromBody] BalancecheckRequest request)
    {
        Logger.LogInformation("Yellow BalanceCheck request: {@Request}", request);
        return Ok(new BalancecheckResponse { Result = 1, Personid = request.Personid });
    }
}
