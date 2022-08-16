using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/verifyqrcode.php")]
[ApiController]
public class VerifyQrCodeController : ControllerBase
{
    private readonly ILogger<VerifyQrCodeController> logger;
    public VerifyQrCodeController(ILogger<VerifyQrCodeController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerifyQrCode([FromBody] VerifyQrcodeRequest request)
    {
        logger.LogInformation("VerifyQrCode request : {Request}", request.Stringify());

        var response = new VerifyQrcodeResponse
        {
            Result = 1,
            QrcodeId   = 1
        };

        return Ok(response);
    }
}