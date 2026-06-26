namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;

[ApiController]
public sealed class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost(MomoiroRoutePrefixes.Game + "/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Momoiro MyDonEntry request: {@Request}", request);

        var common = await Mediator.Send(
            new AddMyDonEntryCommand(GameEra.Momoiro, request.AccessCode, request.MydonName, 0),
            HttpContext.RequestAborted);

        return Ok(new MydonEntryResponse
        {
            Result = common.Result,
            ComSvrResult = common.ComSvrResult,
            Baid = common.Baid,
            AccessCode = common.AccessCode,
            IsPublish = true,
            RegCountryId = "JPN",
            PurposeId = 1,
            RegionId = 1,
            MydonName = common.MydonName,
            RewardPtn = request.RewardPtn,
            ContentInfo = new byte[Ac15EraProfiles.Momoiro.Limits.ContentInfoBytes]
        });
    }
}
