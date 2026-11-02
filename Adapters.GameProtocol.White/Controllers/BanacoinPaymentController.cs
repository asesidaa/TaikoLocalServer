namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/banacoinpayment.php")]
public class BanacoinPaymentController : BaseProtocolController<BanacoinPaymentController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinPayment([FromBody] BanacoinpaymentRequest request)
    {
        Logger.LogInformation("White BanacoinPayment request: {@Request}", request);
        return Ok(new BanacoinpaymentResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = "Ok",
            Chid = "1"
        });
    }
}
