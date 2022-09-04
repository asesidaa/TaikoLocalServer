namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getaidata.php")]
[ApiController]
public class GetAiDataController : BaseController<GetAiDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetAiData([FromBody] GetAiDataRequest request)
    {
        Logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var response = new GetAiDataResponse
        {
            Result  = 1,
            TotalWinnings = 1,
            InputMedian = "1",
            InputVariance = "0.576389"
        };

        return Ok(response);
    }
}