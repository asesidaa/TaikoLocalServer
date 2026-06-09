namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/banacoinpayment.php")]
public class BanacoinPaymentController : BaseProtocolController<BanacoinPaymentController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinPayment([FromBody] BanacoinpaymentRequest request)
    {
        Logger.LogInformation("Yellow BanacoinPayment request: {@Request}", request);
        return Ok(new BanacoinpaymentResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = "Ok",
            Chid = "1"
        });
    }
}
