namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class TournamentCheckController : BaseController<TournamentCheckController>
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
    
    [HttpPost("/v12r00_cn/chassis/tournamentcheck.php")]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheckCN00([FromBody] Models.CN00.TournamentcheckRequest request)
    {
        Logger.LogInformation("TournamentCheck request : {Request}", request.Stringify());

        var response = new Models.CN00.TournamentcheckResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}