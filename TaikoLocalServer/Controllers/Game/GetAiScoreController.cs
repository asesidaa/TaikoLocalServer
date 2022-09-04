namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getaiscore.php")]
[ApiController]
public class GetAiScoreController : BaseController<GetAiScoreController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetAiScore([FromBody] GetAiScoreRequest request)
    {
        Logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var response = new GetAiScoreResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}