namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Yellow MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.Yellow, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            MydonName = common.MydonName,
            ContentInfo = new byte[Ac15EraProfiles.Yellow.Limits.ContentInfoBytes],
            Personid = "1"
        });
    }
}
