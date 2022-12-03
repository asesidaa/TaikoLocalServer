namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/tournamentcheck.php")]
[ApiController]
public class TournamentCheckController : BaseController<TournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("TournamentCheck request : {Request}", request.Stringify());

        var response = new TournamentcheckResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}