namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route(WhiteRoutePrefixes.Final + "/tournamentcheck.php")]
[Route(WhiteRoutePrefixes.Compatibility + "/tournamentcheck.php")]
public sealed class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation(
            "White scaffold tournamentcheck.php request: ChassisId={ChassisId}, ShopId={ShopId}, KitId={KitId}",
            request.ChassisId,
            request.ShopId,
            request.KitId);
        return Ok(new TournamentcheckResponse { Result = 1 });
    }
}
