namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r00/chassis/mydonentry.php")]
[Route("/v08r01/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Red MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.Red, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            MydonName = common.MydonName,
            ContentInfo = new byte[Ac15EraProfiles.Red.Limits.ContentInfoBytes],
            Personid = "1"
        });
    }
}
