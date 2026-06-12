using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Adapters.GameProtocol.Red.Wire;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/playresult.php")]
public sealed class RedPlayResultProbeController : BaseProtocolController<RedPlayResultProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Red route probe playresult.php request: {@Request}", request);
        return Ok(new PlayResultResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/banacoinerrorlog.php")]
public sealed class RedBanacoinErrorLogProbeController : BaseProtocolController<RedBanacoinErrorLogProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinErrorLog([FromBody] BanacoinerrorlogRequest request)
    {
        Logger.LogInformation("Red route probe banacoinerrorlog.php request: {@Request}", request);
        return Ok(new BanacoinerrorlogResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/baidcheck.php")]
public sealed class RedBaidCheckProbeController : BaseProtocolController<RedBaidCheckProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Red route probe baidcheck.php request: {@Request}", request);
        return Ok(new BAIDResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/mydonentry.php")]
public sealed class RedMydonEntryProbeController : BaseProtocolController<RedMydonEntryProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult MydonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("Red route probe mydonentry.php request: {@Request}", request);
        return Ok(new MydonEntryResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/userdata.php")]
public sealed class RedUserDataProbeController : BaseProtocolController<RedUserDataProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("Red route probe userdata.php request: {@Request}", request);
        return Ok(new UserDataResponse
        {
            Result = 1,
            AryFavoriteSongNoes = [],
            AryRecentSongNoes = [],
            RecommendBestSongs = []
        });
    }
}

[ApiController]
[Route("/v08r01/chassis/challengecompe.php")]
public sealed class RedChallengeCompeProbeController : BaseProtocolController<RedChallengeCompeProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult ChallengeCompe([FromBody] ChallengeCompeRequest request)
    {
        Logger.LogInformation("Red route probe challengecompe.php request: {@Request}", request);
        return Ok(new ChallengeCompeResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/balancecheck.php")]
public sealed class RedBalanceCheckProbeController : BaseProtocolController<RedBalanceCheckProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BalanceCheck([FromBody] BalancecheckRequest request)
    {
        Logger.LogInformation("Red route probe balancecheck.php request: {@Request}", request);
        return Ok(new BalancecheckResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = "Ok"
        });
    }
}

[ApiController]
[Route("/v08r01/chassis/banacoinpayment.php")]
public sealed class RedBanacoinPaymentProbeController : BaseProtocolController<RedBanacoinPaymentProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult BanacoinPayment([FromBody] BanacoinpaymentRequest request)
    {
        Logger.LogInformation("Red route probe banacoinpayment.php request: {@Request}", request);
        return Ok(new BanacoinpaymentResponse
        {
            Result = 1,
            Personid = request.Personid,
            BnidResult = "Ok",
            Chid = "1"
        });
    }
}

[ApiController]
[Route("/v08r01/chassis/crownsdata.php")]
public sealed class RedCrownsDataProbeController : BaseProtocolController<RedCrownsDataProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("Red route probe crownsdata.php request: {@Request}", request);
        return Ok(new CrownsDataResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/recommend.php")]
public sealed class RedRecommendProbeController : BaseProtocolController<RedRecommendProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation("Red route probe recommend.php request: {@Request}", request);
        return Ok(new RecommendResponse
        {
            Result = 1,
            RecommendBestSongs = []
        });
    }
}

[ApiController]
[Route("/v08r01/chassis/selfbest.php")]
public sealed class RedSelfBestProbeController : BaseProtocolController<RedSelfBestProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("Red route probe selfbest.php request: {@Request}", request);
        return Ok(new SelfBestResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/heartbeat.php")]
public sealed class RedHeartbeatProbeController : BaseProtocolController<RedHeartbeatProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
    {
        Logger.LogInformation("Red route probe heartbeat.php request: {@Request}", request);
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
[Route("/v08r01/chassis/rewardcardcheck.php")]
public sealed class RedRewardCardCheckProbeController : BaseProtocolController<RedRewardCardCheckProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardCardCheck([FromBody] RewardcardcheckRequest request)
    {
        Logger.LogInformation("Red route probe rewardcardcheck.php request: {@Request}", request);
        return Ok(new RewardcardcheckResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/rewardexecution.php")]
public sealed class RedRewardExecutionProbeController : BaseProtocolController<RedRewardExecutionProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult RewardExecution([FromBody] RewardexecutionRequest request)
    {
        Logger.LogInformation("Red route probe rewardexecution.php request: {@Request}", request);
        return Ok(new RewardexecutionResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/initialdatacheck.php")]
public sealed class RedInitialDataCheckProbeController : BaseProtocolController<RedInitialDataCheckProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Red route probe initialdatacheck.php request: {@Request}", request);
        return Ok(new InitialdatacheckResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/tournamentcheck.php")]
public sealed class RedTournamentCheckProbeController : BaseProtocolController<RedTournamentCheckProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult TournamentCheck([FromBody] TournamentcheckRequest request)
    {
        Logger.LogInformation("Red route probe tournamentcheck.php request: {@Request}", request);
        return Ok(new TournamentcheckResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/bookkeeping.php")]
public sealed class RedBookkeepingProbeController : BaseProtocolController<RedBookkeepingProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
    {
        Logger.LogInformation("Red route probe bookkeeping.php request: {@Request}", request);
        return Ok(new BookKeepingResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/coinsetting.php")]
public sealed class RedCoinsettingProbeController : BaseProtocolController<RedCoinsettingProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Coinsetting([FromBody] CoinsettingRequest request)
    {
        Logger.LogInformation("Red route probe coinsetting.php request: {@Request}", request);
        return Ok(new CoinsettingResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/gettelop.php")]
public sealed class RedGettelopProbeController : BaseProtocolController<RedGettelopProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Gettelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("Red route probe gettelop.php request: {@Request}", request);
        return Ok(new GettelopResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/getfolder.php")]
public sealed class RedGetfolderProbeController : BaseProtocolController<RedGetfolderProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Getfolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Red route probe getfolder.php request: {@Request}", request);
        return Ok(new GetfolderResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/taikojuku.php")]
public sealed class RedTaikojukuProbeController : BaseProtocolController<RedTaikojukuProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Taikojuku([FromBody] TaikojukuRequest request)
    {
        Logger.LogInformation("Red route probe taikojuku.php request: {@Request}", request);
        return Ok(new TaikojukuResponse { Result = 1 });
    }
}

[ApiController]
[Route("/v08r01/chassis/headclerk2.php")]
public sealed class RedHeadClerk2ProbeController : BaseProtocolController<RedHeadClerk2ProbeController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult HeadClerk2([FromBody] HeadClerk2Request request)
    {
        Logger.LogInformation("Red route probe headclerk2.php request: {@Request}", request);
        return Ok(new HeadClerk2Response { Result = 1 });
    }
}
