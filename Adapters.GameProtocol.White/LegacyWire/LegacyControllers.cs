using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/baidcheck.php")]
public sealed class LegacyBaidController : BaseProtocolController<LegacyBaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("White legacy BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.White, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
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
            LegacyBaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            LegacyBaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            LegacyBaidResponseMapper.Apply(inventory, response);
        }

        if (common.DanStatus is { } dan)
        {
            LegacyBaidResponseMapper.Apply(dan, response);
        }

        if (common.CompatibilityProfile is { } compatibility)
        {
            LegacyBaidResponseMapper.Apply(compatibility, response);
        }

        if (common.RewardProgress is { } reward)
        {
            LegacyBaidResponseMapper.Apply(reward, response);
        }
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/bookkeeping.php")]
public sealed class LegacyBookkeepingController : BaseProtocolController<LegacyBookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation(
            "White legacy bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/crownsdata.php")]
public sealed class LegacyCrownsDataController(ITaikoDbContext context, IGameDataCatalog gameDataService)
    : BaseProtocolController<LegacyCrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("White legacy CrownsData request: {@Request}", request);
        var bestRows = await context.SongBestDataWhite
            .Where(row => row.Baid == request.Baid)
            .ToListAsync(HttpContext.RequestAborted);
        var white = gameDataService.White();
        var inflated = CrownsDataMappers.BuildRawInflatedBody(bestRows, white);

        return Ok(new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = white.SongHashVersion,
            HashCrownFlg = inflated
        });
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/getfolder.php")]
public sealed class LegacyGetFolderController : BaseProtocolController<LegacyGetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("White legacy GetFolder request: {@Request}", request);
        var response = await Mediator.Send(
            new GetFolderQuery(GameEra.White, request.FolderIds ?? []),
            HttpContext.RequestAborted);
        return Ok(LegacyFolderDataMappers.Map(response));
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/gettelop.php")]
public sealed class LegacyGetTelopController : BaseProtocolController<LegacyGetTelopController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("White legacy GetTelop request: {@Request}", request);
        var response = await Mediator.Send(
            new GetTelopQuery(GameEra.White, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(LegacyGetTelopMappers.Map(response));
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/heartbeat.php")]
public sealed class LegacyHeartbeatController : BaseProtocolController<LegacyHeartbeatController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation(
            "White legacy heartbeat.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1
        });
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/initialdatacheck.php")]
public sealed class LegacyInitialDataCheckController : BaseProtocolController<LegacyInitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("White legacy InitialDataCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.White), HttpContext.RequestAborted);
        return Ok(LegacyInitialDataMappers.Map(common));
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/mydonentry.php")]
public sealed class LegacyMyDonEntryController : BaseProtocolController<LegacyMyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("White legacy MyDonEntry request: {@Request}", request);

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
            ContentInfo = new byte[Ac15EraProfiles.White.Limits.ContentInfoBytes]
        });
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/playresult.php")]
public sealed class LegacyPlayResultController : BaseProtocolController<LegacyPlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("White legacy PlayResult request: {@Request}", request);
        var playResult = LegacyPlayResultMappers.Map(request);

        var result = await Mediator.Send(
            new UpdateAc15PlayResultCommand(request.Baid, GameEra.White, playResult),
            HttpContext.RequestAborted);

        return Ok(LegacyPlayResultMappers.Map(result));
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/recommend.php")]
public sealed class LegacyRecommendController : BaseProtocolController<LegacyRecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("White legacy Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.White, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(LegacyRecommendMappers.Map(common));
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/selfbest.php")]
public sealed class LegacySelfBestController : BaseProtocolController<LegacySelfBestController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("White legacy SelfBest request: {@Request}", request);
        var common = await Mediator.Send(
            new GetSelfBestQuery(request.Baid, GameEra.White, request.Level, request.ArySongNoes ?? []),
            HttpContext.RequestAborted);
        return Ok(LegacySelfBestMappers.Map(common));
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/taikojuku.php")]
public sealed class LegacyTaikojukuController : BaseProtocolController<LegacyTaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("White legacy Taikojuku request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.White, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(LegacyTaikojukuMappers.Map(common));
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/tournamentcheck.php")]
public sealed class LegacyTournamentCheckController : BaseProtocolController<LegacyTournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation(
            "White legacy tournamentcheck.php request: ChassisId={ChassisId}, ShopId={ShopId}, KitId={KitId}",
            request.ChassisId,
            request.ShopId,
            request.KitId);
        return Ok(new TournamentcheckResponse { Result = 1 });
    }
}

[ApiController]
[Route(WhiteRoutePrefixes.Compatibility + "/userdata.php")]
public sealed class LegacyUserDataController : BaseProtocolController<LegacyUserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("White legacy UserData request: {@Request}", request);
        var common = await Mediator.Send(new Ac15UserDataQuery(request.Baid, GameEra.White), HttpContext.RequestAborted);
        var response = new UserDataResponse
        {
            Result = common.Result
        };
        ApplySections(common, response);

        return Ok(response);
    }

    private static void ApplySections(Ac15UserDataResponse common, UserDataResponse response)
    {
        LegacyUserDataMappers.Apply(common.SongFlags, response);
        LegacyUserDataMappers.Apply(common.SongLists, response);
        LegacyUserDataMappers.Apply(common.Recommendations, response);
        LegacyUserDataMappers.Apply(common.Counters, response);
        LegacyUserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            LegacyUserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            LegacyUserDataMappers.Apply(tutorial, response);
        }

        if (common.Reward is { } reward)
        {
            LegacyUserDataMappers.Apply(reward, response);
        }
    }
}
