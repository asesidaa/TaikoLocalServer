namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetTelopController : BaseController<GetTelopController>
{
    [HttpPost("/v12r08_ww/chassis/gettelop_o0cb2z3e.php")]
    [Produces("application/protobuf")]
    public IActionResult GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("GetTelop request : {Request}", request.Stringify());

        var startDateTime = DateTime.Now - TimeSpan.FromDays(999.0);
        var endDateTime = DateTime.Now + TimeSpan.FromDays(999.0);

        var response = new GettelopResponse
        {
            Result = 1,
            StartDatetime = startDateTime.ToString(Constants.DateTimeFormat),
            EndDatetime = endDateTime.ToString(Constants.DateTimeFormat),
            Telop = "Hello WW08",
            VerupNo = 1
        };

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/gettelop.php")]
    [Produces("application/protobuf")]
    public IActionResult GetTelopCN00([FromBody] Models.CN00.GettelopRequest request)
    {
        Logger.LogInformation("GetTelop request : {Request}", request.Stringify());

        var startDateTime = DateTime.Now - TimeSpan.FromDays(999.0);
        var endDateTime = DateTime.Now + TimeSpan.FromDays(999.0);

        var response = new Models.CN00.GettelopResponse
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