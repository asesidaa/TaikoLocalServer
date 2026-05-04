namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class SetAnyStringController : BaseProtocolController<SetAnyStringController>
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
}
