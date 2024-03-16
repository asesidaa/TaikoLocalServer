using TaikoLocalServer.Models.v3209;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class HeadClerk2Controller : BaseController<HeadClerk2Controller>
{
    [HttpPost("/v12r00_cn/chassis/headclerk2.php")] 
    [Produces("application/protobuf")]
    public IActionResult UploadHeadClert2([FromBody] HeadClerk2Request request)
    {
        Logger.LogInformation("HeadClerk2 request : {Request}", request.Stringify());
        var response = new HeadClerk2Response
        {
            Result = 1
        };
        return Ok(response);
    }
}