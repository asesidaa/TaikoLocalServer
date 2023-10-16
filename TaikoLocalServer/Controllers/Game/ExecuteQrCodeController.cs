namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r00_cn/chassis/executeqrcode.php")]
[ApiController]
public class ExecuteQrCodeController : BaseController<ExecuteQrCodeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ExecuteQrCode([FromBody] ExecuteQrcodeRequest request)
    {
        Logger.LogInformation("ExecuteQrcode request : {Request}", request.Stringify());

        var response = new ExecuteQrcodeResponse
        {
            QrcodeId = 1,
            Result = 1
        };

        return Ok(response);
    }
}