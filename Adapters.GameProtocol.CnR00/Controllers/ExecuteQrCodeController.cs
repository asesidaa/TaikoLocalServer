namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class ExecuteQrCodeController : BaseProtocolController<ExecuteQrCodeController>
{
    [HttpPost("/v12r00_cn/chassis/executeqrcode.php")]
    [Produces("application/protobuf")]
    public IActionResult ExecuteQrCodeCN00([FromBody] ExecuteQrcodeRequest request)
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
