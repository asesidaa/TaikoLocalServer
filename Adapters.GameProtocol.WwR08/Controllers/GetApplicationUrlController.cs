namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetApplicationUrlController : BaseProtocolController<GetApplicationUrlController>
{

    [HttpPost("/v12r08_ww/chassis/getapplicationurl.php")]
    [Produces("application/protobuf")]
    public IActionResult GetApplicationUrl([FromBody] GetApplicationUrlRequest request)
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
