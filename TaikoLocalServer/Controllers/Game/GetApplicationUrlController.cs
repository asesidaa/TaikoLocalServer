namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetApplicationUrlController : BaseController<GetApplicationUrlController>
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
    
    [HttpPost("/v12r00_cn/chassis/getapplicationurl.php")]
    [Produces("application/protobuf")]
    public IActionResult GetApplicationUrlCN00([FromBody] Models.CN00.GetApplicationUrlRequest request)
    {
        Logger.LogInformation("GetApplicationUrl request : {Request}", request.Stringify());

        var response = new Models.CN00.GetApplicationUrlResponse
        {
            Result = 1,
            ApplicationUrl = $"{HttpContext.Request.Host.Value}/app"
        };

        return Ok(response);
    }
    
}