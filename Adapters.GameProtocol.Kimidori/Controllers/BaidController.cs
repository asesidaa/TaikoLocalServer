namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/baidcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Kimidori BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.Kimidori, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            Logger.LogInformation("New Kimidori user with access code {AccessCode}", request.AccessCode);

            return Ok(new BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = common.Baid
            });
        }

        var response = new BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            AccessCode = request.AccessCode,
            IsPublish = true,
            PlayerType = 0,
            ComSvrResult = 1,
            RegCountryId = "JPN",
            MbId = 1,
            PurposeId = 1,
            RegionId = 1,
            ContentInfo = new byte[Ac15EraProfiles.Kimidori.Limits.ContentInfoBytes]
        };
        ApplySections(common, response);

        return Ok(response);
    }

    private static void ApplySections(Ac15BaidResponse common, BAIDResponse response)
    {
        if (common.Identity is { } identity)
        {
            BaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            BaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            BaidResponseMapper.Apply(inventory, response);
        }

        if (common.DanStatus is { } dan)
        {
            BaidResponseMapper.Apply(dan, response);
        }

        if (common.RewardProgress is { } reward)
        {
            BaidResponseMapper.Apply(reward, response);
        }
    }
}
