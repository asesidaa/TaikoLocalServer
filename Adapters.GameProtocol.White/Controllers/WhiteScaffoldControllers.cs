namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/playresult.php")]
public sealed class WhitePlayResultController : BaseProtocolController<WhitePlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation(
            "White scaffold playresult.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}, PlayMode={PlayMode}, StageCount={StageCount}",
            request.Baid,
            request.ChassisId,
            request.ShopId,
            request.PlayMode,
            request.AryStageInfoes.Count);
        return Ok(new PlayResultResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/baidcheck.php")]
public sealed class WhiteBaidController : BaseProtocolController<WhiteBaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation(
            "White scaffold baidcheck.php request: DeviceType={DeviceType}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.DeviceType,
            request.ChassisId,
            request.ShopId);
        return Ok(new BAIDResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/mydonentry.php")]
public sealed class WhiteMydonEntryController : BaseProtocolController<WhiteMydonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation(
            "White scaffold mydonentry.php request: DeviceType={DeviceType}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.DeviceType,
            request.ChassisId,
            request.ShopId);
        return Ok(new MydonEntryResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/userdata.php")]
public sealed class WhiteUserDataController : BaseProtocolController<WhiteUserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation(
            "White scaffold userdata.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.Baid,
            request.ChassisId,
            request.ShopId);
        return Ok(new UserDataResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/crownsdata.php")]
public sealed class WhiteCrownsDataController : BaseProtocolController<WhiteCrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation(
            "White scaffold crownsdata.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}",
            request.Baid,
            request.ChassisId,
            request.ShopId);
        return Ok(new CrownsDataResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/recommend.php")]
public sealed class WhiteRecommendController : BaseProtocolController<WhiteRecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation(
            "White scaffold recommend.php request: ChassisId={ChassisId}, ShopId={ShopId}, GenderType={GenderType}, PlayerAge={PlayerAge}",
            request.ChassisId,
            request.ShopId,
            request.GenderType,
            request.PlayerAge);
        return Ok(new RecommendResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/selfbest.php")]
public sealed class WhiteSelfBestController : BaseProtocolController<WhiteSelfBestController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation(
            "White scaffold selfbest.php request: Baid={Baid}, ChassisId={ChassisId}, ShopId={ShopId}, Level={Level}, SongCount={SongCount}",
            request.Baid,
            request.ChassisId,
            request.ShopId,
            request.Level,
            request.ArySongNoes?.Length ?? 0);
        return Ok(new SelfBestResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/heartbeat.php")]
public sealed class WhiteHeartbeatController : BaseProtocolController<WhiteHeartbeatController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation(
            "White scaffold heartbeat.php request: ChassisId={ChassisId}, ShopId={ShopId}",
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
[Route("/v07r00/chassis/initialdatacheck.php")]
public sealed class WhiteInitialDataCheckController : BaseProtocolController<WhiteInitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation(
            "White scaffold initialdatacheck.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new InitialdatacheckResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/tournamentcheck.php")]
public sealed class WhiteTournamentCheckController : BaseProtocolController<WhiteTournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation(
            "White scaffold tournamentcheck.php request: ChassisId={ChassisId}, ShopId={ShopId}, KitId={KitId}",
            request.ChassisId,
            request.ShopId,
            request.KitId);
        return Ok(new TournamentcheckResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/bookkeeping.php")]
public sealed class WhiteBookkeepingController : BaseProtocolController<WhiteBookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation(
            "White scaffold bookkeeping.php request: ChassisId={ChassisId}, ShopId={ShopId}",
            request.ChassisId,
            request.ShopId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/gettelop.php")]
public sealed class WhiteGetTelopController : BaseProtocolController<WhiteGetTelopController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation(
            "White scaffold gettelop.php request: ChassisId={ChassisId}, ShopId={ShopId}, TelopId={TelopId}",
            request.ChassisId,
            request.ShopId,
            request.TelopId);
        return Ok(new GettelopResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/getfolder.php")]
public sealed class WhiteGetFolderController : BaseProtocolController<WhiteGetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation(
            "White scaffold getfolder.php request: ChassisId={ChassisId}, ShopId={ShopId}, HddVer={HddVer}, FolderCount={FolderCount}",
            request.ChassisId,
            request.ShopId,
            request.HddVer,
            request.FolderIds?.Length ?? 0);
        return Ok(new GetfolderResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v07r00/chassis/taikojuku.php")]
public sealed class WhiteTaikojukuController : BaseProtocolController<WhiteTaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation(
            "White scaffold taikojuku.php request: ChassisId={ChassisId}, ShopId={ShopId}, GetDanCount={GetDanCount}",
            request.ChassisId,
            request.ShopId,
            request.GetDans?.Length ?? 0);
        return Ok(new TaikojukuResponse { Result = 1 });
    }
}
