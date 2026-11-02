namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/coinsetting.php")]
public class CoinSettingController : BaseProtocolController<CoinSettingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CoinSetting([FromBody] CoinsettingRequest request)
    {
        Logger.LogInformation("Yellow CoinSetting request from {ChassisId}", request.ChassisId);
        return Ok(new CoinsettingResponse { Result = 1 });
    }
}
