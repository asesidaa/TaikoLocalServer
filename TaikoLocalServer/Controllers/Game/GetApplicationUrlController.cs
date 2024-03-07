namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r08_ww/chassis/getapplicationurl.php")]
[ApiController]
public class GetApplicationUrlController : BaseController<GetApplicationUrlController>
{
    private const string APPLICATION_URL = "vsapi.taiko-p.jp";

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetApplicationUrl([FromBody] GetApplicationUrlRequest request)
    {
        Logger.LogInformation("GetApplicationUrl request : {Request}", request.Stringify());

        var response = new GetApplicationUrlResponse
        {
            Result = 1,
            ApplicationUrl = APPLICATION_URL
        };

        return Ok(response);
    }
}