namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00/chassis/balancecheck.php")]
[Route("/v08r01/chassis/balancecheck.php")]
public class BalanceCheckController : BaseProtocolController<BalanceCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BalanceCheck([FromBody] BalancecheckRequest request)
    {
        Logger.LogInformation("Red route probe balancecheck.php request: {@Request}", request);
        return Ok(new BalancecheckResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = "Ok"
        });
    }
}
