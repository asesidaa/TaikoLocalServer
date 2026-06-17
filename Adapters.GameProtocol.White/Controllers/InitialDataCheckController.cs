namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/initialdatacheck.php")]
public sealed class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation(
            "White scaffold initialdatacheck.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new InitialdatacheckResponse { Result = 1 });
    }
}
