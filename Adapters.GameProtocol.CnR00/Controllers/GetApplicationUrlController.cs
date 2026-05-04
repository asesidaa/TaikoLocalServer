namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetApplicationUrlController : BaseProtocolController<GetApplicationUrlController>
{

    [HttpPost("/v12r00_cn/chassis/getapplicationurl.php")]
    [Produces("application/protobuf")]
    public IActionResult GetApplicationUrlCN00([FromBody] GetApplicationUrlRequest request)
    {
        Logger.LogInformation("GetApplicationUrl request : {Request}", request.Stringify());

        var response = new GetApplicationUrlResponse
        {
            Result = 1,
            ApplicationUrl = $"{HttpContext.Request.Host.Value}/app"
        };

        return Ok(response);
    }

}
