namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/gettelop.php")]
[ApiController]
public class GetTelopController : BaseController<GetTelopController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("GetTelop request : {Request}", request.Stringify());

        var startDateTime = DateTime.Now - TimeSpan.FromDays(999.0);
        var endDateTime = DateTime.Now + TimeSpan.FromDays(999.0);

        var response = new GettelopResponse
        {
            Result = 1,
            StartDatetime = startDateTime.ToString(Constants.DATE_TIME_FORMAT),
            EndDatetime = endDateTime.ToString(Constants.DATE_TIME_FORMAT),
            Telop = "Hello world",
            VerupNo = 1
        };

        return Ok(response);
    }
}