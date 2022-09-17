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
            TotalWinnings = 0
        };

        return Ok(response);
    }
}