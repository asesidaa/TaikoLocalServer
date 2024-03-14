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
    public IActionResult GetApplicationUrl3209([FromBody] Models.v3209.GetApplicationUrlRequest request)
    {
        Logger.LogInformation("GetApplicationUrl request : {Request}", request.Stringify());

        var response = new Models.v3209.GetApplicationUrlResponse
        {
            Result = 1,
            ApplicationUrl = $"{HttpContext.Request.Host.Value}/app"
        };

        return Ok(response);
    }
    
}