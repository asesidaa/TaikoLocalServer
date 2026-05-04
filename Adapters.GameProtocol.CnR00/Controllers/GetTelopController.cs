namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost("/v12r00_cn/chassis/gettelop.php")]
    [Produces("application/protobuf")]
    public IActionResult GetTelopCN00([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("GetTelop request : {Request}", request.Stringify());

        var startDateTime = DateTime.Now - TimeSpan.FromDays(999.0);
        var endDateTime = DateTime.Now + TimeSpan.FromDays(999.0);

        var response = new GettelopResponse
        {
            Result = 1,
            StartDatetime = startDateTime.ToString(Constants.DateTimeFormat),
            EndDatetime = endDateTime.ToString(Constants.DateTimeFormat),
            Telop = "Hello CN00",
            VerupNo = 1
        };

        return Ok(response);
    }
}
