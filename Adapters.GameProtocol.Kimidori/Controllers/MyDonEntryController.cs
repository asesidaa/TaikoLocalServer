namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalMydonEntry([FromBody] FinalWire.MydonEntryRequest request)
    {
        Logger.LogInformation("Kimidori final MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.Kimidori, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new FinalWire.MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            MbId = 1,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            RegCountryId = "JPN",
            PurposeId = 1,
            RegionId = 1,
            MydonName = common.MydonName,
            RewardPtn = request.RewardPtn,
            ContentInfo = new byte[Ac15EraProfiles.Kimidori.Limits.ContentInfoBytes]
        });
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Kimidori MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.Kimidori, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            MbId = 1,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            RegCountryId = "JPN",
            PurposeId = 1,
            RegionId = 1,
            MydonName = common.MydonName,
            RewardPtn = request.RewardPtn,
            ContentInfo = new byte[Ac15EraProfiles.Kimidori.Limits.ContentInfoBytes]
        });
    }
}
