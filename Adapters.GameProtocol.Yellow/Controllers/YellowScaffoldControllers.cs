namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r00/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Yellow InitialDataCheck request from {ChassisId}", request.ChassisId);
        return Ok(new InitialdatacheckResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/tournamentcheck.php")]
public class TournamentCheckController : BaseProtocolController<TournamentCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("Yellow TournamentCheck request from {ChassisId}", request.ChassisId);
        return Ok(new TournamentcheckResponse { Result = 1 });
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
    public IActionResult GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("Yellow GetTelop request from {ChassisId}", request.ChassisId);
        return Ok(new GettelopResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/getfolder.php")]
public class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Yellow GetFolder request from {ChassisId}", request.ChassisId);
        return Ok(new GetfolderResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/taikojuku.php")]
public class TaikojukuController : BaseProtocolController<TaikojukuController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("Yellow Taikojuku request from {ChassisId}", request.ChassisId);
        return Ok(new TaikojukuResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v09r00/chassis/getitemshopinfo.php")]
public class GetItemShopInfoController : BaseProtocolController<GetItemShopInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetItemShopInfo([FromBody] GetitemshopinfoRequest request)
    {
        Logger.LogInformation("Yellow GetItemShopInfo request from {ChassisId}", request.ChassisId);
        return Ok(new GetitemshopinfoResponse { Result = 1 });
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
    public IActionResult ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Yellow ChallengeCompe request from {ChassisId}", request.ChassisId);
        return Ok(new ChallengeCompeResponse { Result = 1 });
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
    public IActionResult Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Yellow Recommend request from {ChassisId}", request.ChassisId);
        return Ok(new RecommendResponse { Result = 1 });
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
