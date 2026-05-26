namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/balancecheck.php")]
public class BalanceCheckController : BaseProtocolController<BalanceCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BalanceCheck([FromBody] BalancecheckRequest request)
    {
        Logger.LogInformation("Blue BalanceCheck request: {Request}", request.Stringify());
        return Ok(new BalancecheckResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = string.Empty,
            CoinCoupon = 0
        });
    }
}
