namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00/chassis/banacoinpayment.php")]
[Route("/v08r01/chassis/banacoinpayment.php")]
public class BanacoinPaymentController : BaseProtocolController<BanacoinPaymentController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinPayment([FromBody] BanacoinpaymentRequest request)
    {
        Logger.LogInformation("Red route probe banacoinpayment.php request: {@Request}", request);
        return Ok(new BanacoinpaymentResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = "Ok",
            Chid = "1"
        });
    }
}
