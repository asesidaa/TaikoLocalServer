using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/baidcheck.php")]
    [Produces("application/protobuf")]
    public IActionResult BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation(
            "Momoiro baidcheck.php scaffold request: ChassisId={ChassisId}, ShopId={ShopId}, CountryId={CountryId}",
            request.ChassisId,
            request.ShopId,
            request.CountryId);

        return Ok(new BAIDResponse { Result = 1 });
    }
}
