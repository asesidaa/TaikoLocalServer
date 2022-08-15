using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/executeqrcode.php")]
[ApiController]
public class ExecuteQrCodeController : ControllerBase
{
    private readonly ILogger<ExecuteQrCodeController> logger;
    public ExecuteQrCodeController(ILogger<ExecuteQrCodeController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ExecuteQrCode([FromBody] ExecuteQrcodeRequest request)
    {
        logger.LogInformation("ExecuteQrcode request : {Request}", request.Stringify());

        var response = new ExecuteQrcodeResponse
        {
            QrcodeId = 1,
            Result = 1
        };

        return Ok(response);
    }
}