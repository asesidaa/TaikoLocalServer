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
    public IActionResult ExecuteQrCode3209([FromBody] Models.v3209.ExecuteQrcodeRequest request)
    {
        Logger.LogInformation("ExecuteQrcode request : {Request}", request.Stringify());

        var response = new Models.v3209.ExecuteQrcodeResponse
        {
            QrcodeId = 1,
            Result = 1
        };

        return Ok(response);
    }
}