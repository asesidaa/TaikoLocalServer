using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/baidcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("White BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.White, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            Logger.LogInformation("New White user with access code {AccessCode}", request.AccessCode);

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
            ContentInfo = new byte[Ac15EraProfiles.White.Limits.ContentInfoBytes]
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

        if (common.CompatibilityProfile is { } compatibility)
        {
            BaidResponseMapper.Apply(compatibility, response);
        }

        if (common.RewardProgress is { } reward)
        {
            BaidResponseMapper.Apply(reward, response);
        }
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/baidcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyBaidCheck([FromBody] LegacyWire.BAIDRequest request)
    {
        Logger.LogInformation("White legacy BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.White, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            return Ok(new LegacyWire.BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = common.Baid
            });
        }

        var response = new LegacyWire.BAIDResponse
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
            ContentInfo = new byte[Ac15EraProfiles.White.Limits.ContentInfoBytes]
        };
        ApplyLegacySections(common, response);

        return Ok(response);
    }

    private static void ApplyLegacySections(Ac15BaidResponse common, LegacyWire.BAIDResponse response)
    {
        if (common.Identity is { } identity)
        {
            LegacyWire.LegacyBaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            LegacyWire.LegacyBaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            LegacyWire.LegacyBaidResponseMapper.Apply(inventory, response);
        }

        if (common.DanStatus is { } dan)
        {
            LegacyWire.LegacyBaidResponseMapper.Apply(dan, response);
        }

        if (common.CompatibilityProfile is { } compatibility)
        {
            LegacyWire.LegacyBaidResponseMapper.Apply(compatibility, response);
        }

        if (common.RewardProgress is { } reward)
        {
            LegacyWire.LegacyBaidResponseMapper.Apply(reward, response);
        }
    }
}
