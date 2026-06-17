namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/crownsdata.php")]
public sealed class CrownsDataController : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation(
            "White scaffold crownsdata.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.Baid,
            request.ChassisId,
            request.ShopId);
        return Ok(new CrownsDataResponse { Result = 1 });
    }
}
