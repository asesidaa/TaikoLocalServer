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
            Telop = "Hello 3906",
            VerupNo = 1
        };

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/gettelop.php")]
    [Produces("application/protobuf")]
    public IActionResult GetTelop3209([FromBody] Models.v3209.GettelopRequest request)
    {
        Logger.LogInformation("GetTelop request : {Request}", request.Stringify());

        var startDateTime = DateTime.Now - TimeSpan.FromDays(999.0);
        var endDateTime = DateTime.Now + TimeSpan.FromDays(999.0);

        var response = new Models.v3209.GettelopResponse
        {
            Result = 1,
            StartDatetime = startDateTime.ToString(Constants.DateTimeFormat),
            EndDatetime = endDateTime.ToString(Constants.DateTimeFormat),
            Telop = "Hello 3209",
            VerupNo = 1
        };

        return Ok(response);
    }
}