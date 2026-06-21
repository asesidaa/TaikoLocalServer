using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/tournamentcheck.php")]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation(
            "White scaffold tournamentcheck.php request: ChassisId={ChassisId}, ShopId={ShopId}, KitId={KitId}",
            request.ChassisId,
            request.ShopId,
            request.KitId);
        return Ok(new TournamentcheckResponse { Result = 904 });
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/tournamentcheck.php")]
    [Produces("application/protobuf")]
    public IActionResult LegacyTournamentCheck([FromBody] LegacyWire.TournamentcheckRequest request)
    {
        Logger.LogInformation(
            "White legacy tournamentcheck.php request: ChassisId={ChassisId}, ShopId={ShopId}, KitId={KitId}",
            request.ChassisId,
            request.ShopId,
            request.KitId);
        return Ok(new LegacyWire.TournamentcheckResponse { Result = 904 });
    }
}
