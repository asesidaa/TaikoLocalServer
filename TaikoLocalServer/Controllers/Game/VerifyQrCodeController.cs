namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/verifyqrcode.php")]
[ApiController]
public class VerifyQrCodeController : BaseController<VerifyQrCodeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerifyQrCode([FromBody] VerifyQrcodeRequest request)
    {
        Logger.LogInformation("VerifyQrCode request : {Request}", request.Stringify());

        var response = new VerifyQrcodeResponse
        {
            Result = 1,
            QrcodeId = 1
        };

        return Ok(response);
    }
}