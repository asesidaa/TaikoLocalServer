namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost("/v12r08_ww/chassis/tournamentcheck.php")]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("TournamentCheck request : {Request}", request.Stringify());

        var response = new TournamentcheckResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}
