namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class SetAnyStringController : BaseProtocolController<SetAnyStringController>
{
    [HttpPost("/v12r00_cn/chassis/setanystring.php")]
    [Produces("application/protobuf")]
    public IActionResult SetAnyStringCN00([FromBody] SetAnyStringRequest request)
    {
        Logger.LogInformation("SetAnyString request : {Request}", request.Stringify());

        var response = new SetAnyStringResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}
