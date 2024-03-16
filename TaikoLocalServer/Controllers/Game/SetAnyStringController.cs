namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class SetAnyStringController : BaseController<SetAnyStringController>
{
    [HttpPost("/v12r08_ww/chassis/setanystring_mssxf3bo.php")]
    [Produces("application/protobuf")]
    public IActionResult SetAnyString([FromBody] SetAnyStringRequest request)
    {
        Logger.LogInformation("SetAnyString request : {Request}", request.Stringify());

        var response = new SetAnyStringResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/setanystring.php")]
    [Produces("application/protobuf")]
    public IActionResult SetAnyString3209([FromBody] Models.v3209.SetAnyStringRequest request)
    {
        Logger.LogInformation("SetAnyString request : {Request}", request.Stringify());

        var response = new Models.v3209.SetAnyStringResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}