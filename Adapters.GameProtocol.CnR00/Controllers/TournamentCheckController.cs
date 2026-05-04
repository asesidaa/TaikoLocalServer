namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost("/v12r00_cn/chassis/tournamentcheck.php")]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheckCN00([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("TournamentCheck request : {Request}", request.Stringify());

        var response = new TournamentcheckResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}
