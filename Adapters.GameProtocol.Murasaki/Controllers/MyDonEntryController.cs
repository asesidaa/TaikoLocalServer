namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost(MurasakiRoutePrefixes.Final + "/mydonentry.php")]
    [HttpPost(MurasakiRoutePrefixes.Compatibility + "/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Murasaki MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.Murasaki, request.AccessCode, request.MydonName, 0),
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
            ContentInfo = new byte[Ac15EraProfiles.Murasaki.Limits.ContentInfoBytes]
        });
    }
}
