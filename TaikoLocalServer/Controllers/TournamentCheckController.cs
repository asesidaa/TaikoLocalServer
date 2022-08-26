namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/tournamentcheck.php")]
[ApiController]
public class TournamentCheckController : ControllerBase
{
    private readonly ILogger<TournamentCheckController> logger;
    public TournamentCheckController(ILogger<TournamentCheckController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        logger.LogInformation("TournamentCheck request : {Request}", request.Stringify());

        var response = new TournamentcheckResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}