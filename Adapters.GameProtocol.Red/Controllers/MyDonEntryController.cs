namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Red route probe mydonentry.php request: {@Request}", request);
        return Ok(new MydonEntryResponse { Result = 1 });
    }
}
