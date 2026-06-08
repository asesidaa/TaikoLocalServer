namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/tournamentcheck.php")]
public class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("Yellow TournamentCheck request: {@Request}", request);
        var common = await Mediator.Send(
            new TournamentCheckQuery(GameEra.Yellow, request.KitId),
            HttpContext.RequestAborted);
        return Ok(TournamentMappers.Map(common));
    }
}
