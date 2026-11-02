namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class ExecuteQrCodeController : BaseProtocolController<ExecuteQrCodeController>
{
    [HttpPost("/v12r08_ww/chassis/executeqrcode_rgowsr5m.php")]
    [Produces("application/protobuf")]
    public IActionResult ExecuteQrCode([FromBody] ExecuteQrcodeRequest request)
    {
        Logger.LogInformation("ExecuteQrcode request : {@Request}", request);

        var response = new ExecuteQrcodeResponse
        {
            QrcodeId = 1,
            Result = 1
        };

        return Ok(response);
    }
}
