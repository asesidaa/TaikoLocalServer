namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Blue MyDonEntry request: {Request}", request.Stringify());

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.Blue, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            MydonName = common.MydonName,
            ContentInfo = new byte[BlueProtocolBytes.ContentInfoBytes]
        });
    }
}
