namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/tournamentcheck.php")]
public class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("Blue TournamentCheck request: {Request}", request.Stringify());
        return Ok(new TournamentcheckResponse { Result = 1 });
    }
}
