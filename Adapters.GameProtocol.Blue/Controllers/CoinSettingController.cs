namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/coinsetting.php")]
public class CoinSettingController : BaseProtocolController<CoinSettingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CoinSetting([FromBody] CoinsettingRequest request)
    {
        Logger.LogInformation("Blue CoinSetting request: {@Request}", request);
        return Ok(new CoinsettingResponse { Result = 1 });
    }
}
