using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("White MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.White, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            MydonName = common.MydonName,
            ContentInfo = new byte[Ac15EraProfiles.White.Limits.ContentInfoBytes],
            Personid = "1"
        });
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyMydonEntry([FromBody] LegacyWire.MydonEntryRequest request)
    {
        Logger.LogInformation("White legacy MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.White, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new LegacyWire.MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            MydonName = common.MydonName,
            ContentInfo = new byte[Ac15EraProfiles.White.Limits.ContentInfoBytes]
        });
    }
}
