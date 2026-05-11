namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Green MyDonEntry request: {Request}", request.Stringify());
        return Ok(new MydonEntryResponse
        {
            Result = 1,
            Baid = 1,
            AccessCode = request.AccessCode,
            MydonName = request.MydonName,
            IsPublish = true
        });
    }
}
