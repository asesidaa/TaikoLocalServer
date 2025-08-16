namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class ExecuteQrCodeController : BaseController<ExecuteQrCodeController>
{
    [HttpPost("/v12r08_ww/chassis/executeqrcode_rgowsr5m.php")]
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
    
    [HttpPost("/v12r00_cn/chassis/executeqrcode.php")]
    [Produces("application/protobuf")]
    public IActionResult ExecuteQrCodeCN00([FromBody] Models.CN00.ExecuteQrcodeRequest request)
    {
        Logger.LogInformation("ExecuteQrcode request : {Request}", request.Stringify());

        var response = new Models.CN00.ExecuteQrcodeResponse
        {
            QrcodeId = 1,
            Result = 1
        };

        return Ok(response);
    }
}