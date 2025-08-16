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
    public IActionResult SetAnyStringCN00([FromBody] Models.CN00.SetAnyStringRequest request)
    {
        Logger.LogInformation("SetAnyString request : {Request}", request.Stringify());

        var response = new Models.CN00.SetAnyStringResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}