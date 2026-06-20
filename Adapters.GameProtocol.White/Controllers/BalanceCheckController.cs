namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/balancecheck.php")]
[Route(WhiteRoutePrefixes.Compatibility + "/balancecheck.php")]
public class BalanceCheckController : BaseProtocolController<BalanceCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BalanceCheck([FromBody] BalancecheckRequest request)
    {
        Logger.LogInformation("White BalanceCheck request: {@Request}", request);
        return Ok(new BalancecheckResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = "Ok",
            CoinCoupon = 9999
        });
    }
}
