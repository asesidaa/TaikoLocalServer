namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/getghostscore.php")]
public class GetGhostScoreController : BaseProtocolController<GetGhostScoreController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetGhostScore([FromBody] GetghostscoreRequest request)
    {
        Logger.LogInformation("Green GetGhostScore request: {Request}", request.Stringify());
        return Ok(new GetghostscoreResponse { Result = 1 });
    }
}
