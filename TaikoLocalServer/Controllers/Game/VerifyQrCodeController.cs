namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r00_cn/chassis/verifyqrcode.php")]
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
            QrcodeId = 999999001
        };

        return Ok(response);
    }
}