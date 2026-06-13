namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00_tw/chassis/tournamentcheck.php")]
[Route("/v08r01/chassis/tournamentcheck.php")]
public class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("Red route probe tournamentcheck.php request: {@Request}", request);
        return Ok(new TournamentcheckResponse { Result = 1 });
    }
}
