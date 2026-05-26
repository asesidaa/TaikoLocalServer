namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Blue MyDonEntry request: {Request}", request.Stringify());
        return Ok(new MydonEntryResponse
        {
            Result = 1,
            AccessCode = request.AccessCode,
            IsPublish = true,
            MydonName = request.MydonName
        });
    }
}
