namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[ApiController]
[Route("/v09r00/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Yellow InitialDataCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Yellow), HttpContext.RequestAborted);
        return Ok(InitialDataMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/tournamentcheck.php")]
public class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("Yellow TournamentCheck request: {@Request}", request);
        var common = await Mediator.Send(
            new TournamentCheckQuery(GameEra.Yellow, request.KitId),
            HttpContext.RequestAborted);
        return Ok(TournamentMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/bookkeeping.php")]
public class BookkeepingController : BaseProtocolController<BookkeepingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Yellow Bookkeeping request from {ChassisId}", request.ChassisId);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/coinsetting.php")]
public class CoinSettingController : BaseProtocolController<CoinSettingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CoinSetting([FromBody] CoinsettingRequest request)
    {
        Logger.LogInformation("Yellow CoinSetting request from {ChassisId}", request.ChassisId);
        return Ok(new CoinsettingResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/gettelop.php")]
public class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("Yellow GetTelop request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTelopQuery(GameEra.Yellow, request.TelopId),
            HttpContext.RequestAborted);
        return Ok(GetTelopMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/getfolder.php")]
public class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Yellow GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Yellow, request.FolderIds ?? []),
            HttpContext.RequestAborted);
        return Ok(FolderDataMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/taikojuku.php")]
public class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("Yellow Taikojuku request: {@Request}", request);
        var common = await Mediator.Send(
            new GetTaikojukuQuery(GameEra.Yellow, request.GetDans ?? []),
            HttpContext.RequestAborted);
        return Ok(TaikojukuMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/getitemshopinfo.php")]
public class GetItemShopInfoController : BaseProtocolController<GetItemShopInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetItemShopInfo([FromBody] GetitemshopinfoRequest request)
    {
        Logger.LogInformation("Yellow GetItemShopInfo request: {@Request}", request);
        var common = await Mediator.Send(ItemShopMappers.Map(request), HttpContext.RequestAborted);
        return Ok(ItemShopMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/headclerk2.php")]
public class HeadClerk2Controller : BaseProtocolController<HeadClerk2Controller>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HeadClerk2([FromBody] HeadClerk2Request request)
    {
        Logger.LogInformation("Yellow HeadClerk2 request from {ChassisId}", request.ChassisId);
        return Ok(new HeadClerk2Response { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Yellow PlayResult request from {ChassisId}", request.ChassisId);
        return Ok(new PlayResultResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Yellow BAID request from {ChassisId}", request.ChassisId);
        return Ok(new BAIDResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/mydonentry.php")]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Yellow MyDonEntry request from {ChassisId}", request.ChassisId);
        return Ok(new MydonEntryResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/userdata.php")]
public class UserDataController : BaseProtocolController<UserDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Yellow UserData request from {ChassisId}", request.ChassisId);
        return Ok(new UserDataResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/challengecompe.php")]
public class ChallengeCompeController : BaseProtocolController<ChallengeCompeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Yellow ChallengeCompe request: {@Request}", request);
        var common = await Mediator.Send(
            new GetChallengeCompeQuery(GameEra.Yellow, request.Baid),
            HttpContext.RequestAborted);
        return Ok(ChallengeCompeMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/balancecheck.php")]
public class BalanceCheckController : BaseProtocolController<BalanceCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BalanceCheck([FromBody] BalancecheckRequest request)
    {
        Logger.LogInformation("Yellow BalanceCheck request from {ChassisId}", request.ChassisId);
        return Ok(new BalancecheckResponse { Result = 1, Personid = request.Personid });
    }
}

[ApiController]
[Route("/v09r00/chassis/banacoinpayment.php")]
public class BanacoinPaymentController : BaseProtocolController<BanacoinPaymentController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinPayment([FromBody] BanacoinpaymentRequest request)
    {
        Logger.LogInformation("Yellow BanacoinPayment request from {ChassisId}", request.ChassisId);
        return Ok(new BanacoinpaymentResponse { Result = 1, Personid = request.Personid });
    }
}

[ApiController]
[Route("/v09r00/chassis/banacoinerrorlog.php")]
public class BanacoinErrorLogController : BaseProtocolController<BanacoinErrorLogController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinErrorLog([FromBody] BanacoinerrorlogRequest request)
    {
        Logger.LogInformation("Yellow BanacoinErrorLog request from {ChassisId}", request.ChassisId);
        return Ok(new BanacoinerrorlogResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/getbanacoininfo.php")]
public class GetBanacoinInfoController : BaseProtocolController<GetBanacoinInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetBanacoinInfo([FromBody] GetbanacoininfoRequest request)
    {
        Logger.LogInformation("Yellow GetBanacoinInfo request from {ChassisId}", request.ChassisId);
        return Ok(new GetbanacoininfoResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/crownsdata.php")]
public class CrownsDataController : BaseProtocolController<CrownsDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Yellow CrownsData request from {ChassisId}", request.ChassisId);
        return Ok(new CrownsDataResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/recommend.php")]
public class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Yellow Recommend request: {@Request}", request);
        var common = await Mediator.Send(
            new GetRecommendQuery(GameEra.Yellow, request.GenderType, request.PlayerAge),
            HttpContext.RequestAborted);
        return Ok(RecommendMappers.Map(common));
    }
}

[ApiController]
[Route("/v09r00/chassis/selfbest.php")]
public class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("Yellow SelfBest request from {ChassisId}", request.ChassisId);
        return Ok(new SelfBestResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/heartbeat.php")]
public class HeartbeatController : BaseProtocolController<HeartbeatController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Yellow Heartbeat request from {ChassisId}", request.ChassisId);
        return Ok(new HeartBeatResponse
        {
            Result = 1,
            ComSvrStat = 1,
            GameSvrStat = 1,
            BnidSvrStat = 1,
            BanacoinStat = 1
        });
    }
}

[ApiController]
[Route("/v09r00/chassis/itempurchase.php")]
public class ItemPurchaseController : BaseProtocolController<ItemPurchaseController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ItemPurchase([FromBody] ItempurchaseRequest request)
    {
        Logger.LogInformation("Yellow ItemPurchase request from {ChassisId}", request.ChassisId);
        return Ok(new ItempurchaseResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/rewardcardcheck.php")]
public class RewardCardCheckController : BaseProtocolController<RewardCardCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardCardCheck([FromBody] RewardcardcheckRequest request)
    {
        Logger.LogInformation("Yellow RewardCardCheck request from {ChassisId}", request.ChassisId);
        return Ok(new RewardcardcheckResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/rewardexecution.php")]
public class RewardExecutionController : BaseProtocolController<RewardExecutionController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardExecution([FromBody] RewardexecutionRequest request)
    {
        Logger.LogInformation("Yellow RewardExecution request from {ChassisId}", request.ChassisId);
        return Ok(new RewardexecutionResponse { Result = 1 });
    }
}
