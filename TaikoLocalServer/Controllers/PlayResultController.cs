using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ProtoBuf;
using TaikoLocalServer.Common;
using TaikoLocalServer.Common.Enums;
using TaikoLocalServer.Context;
using TaikoLocalServer.Entities;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/playresult.php")]
[ApiController]
public class PlayResultController : ControllerBase
{
    private readonly ILogger<PlayResultController> logger;

    private readonly TaikoDbContext context;
    
    public PlayResultController(ILogger<PlayResultController> logger, TaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UploadPlayResult([FromBody] PlayResultRequest request)
    {
        logger.LogInformation("PlayResult request : {Request}", request.Stringify());
        var decompressed = GZipBytesUtil.DecompressGZipBytes(request.PlayresultData);

        var playResultData = Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(decompressed));
        
        logger.LogInformation("Play result data {Data}", playResultData.Stringify());

        var userdata = new UserDatum
        {
            Baid = request.BaidConf
        };
        if (context.UserData.Any(datum => datum.Baid == request.BaidConf))
        {
            userdata = context.UserData.First(datum => datum.Baid == request.BaidConf);
        }

        userdata.Title = playResultData.Title;
        userdata.TitlePlateId = playResultData.TitleplateId;
        var costumeData = new List<uint>
        {
            playResultData.AryCurrentCostume.Costume1,
            playResultData.AryCurrentCostume.Costume2,
            playResultData.AryCurrentCostume.Costume3,
            playResultData.AryCurrentCostume.Costume4,
            playResultData.AryCurrentCostume.Costume5
        };
        userdata.CostumeData = JsonSerializer.Serialize(costumeData);
        context.UserData.Update(userdata);

        foreach (var stageData in playResultData.AryStageInfoes)
        {
            var bestData = context.SongBestData
                .FirstOrDefault(datum => datum.Baid == request.BaidConf &&
                                datum.SongId == stageData.SongNo &&
                                datum.Difficulty == (Difficulty)stageData.Level,
                    new SongBestDatum
                    {
                        Baid = request.BaidConf,
                        SongId = stageData.SongNo,
                        Difficulty = (Difficulty)stageData.Level
                    });
            if ((uint)bestData.BestCrown < stageData.PlayResult)
            {
                bestData.BestCrown = (CrownType)stageData.PlayResult;
            }
            if ((uint)bestData.BestScoreRank < stageData.ScoreRank)
            {
                bestData.BestScoreRank = (ScoreRank)stageData.ScoreRank;
            }
            if (bestData.BestScore < stageData.PlayScore)
            {
                bestData.BestScore = stageData.PlayScore;
            }
            if (bestData.BestRate < stageData.ScoreRate)
            {
                bestData.BestRate = stageData.ScoreRate;
            }

            context.SongBestData.Update(bestData);
            var playData = new SongPlayDatum
            {
                Baid = request.BaidConf,
                GoodCount = stageData.GoodCnt,
                OkCount = stageData.OkCnt,
                MissCount = stageData.NgCnt,
                ComboCount = stageData.ComboCnt,
                HitCount = stageData.HitCnt,
                Crown = (CrownType)stageData.PlayResult,
                Score = stageData.PlayScore,
                ScoreRate = stageData.ScoreRate,
                ScoreRank = (ScoreRank)stageData.ScoreRank,
                Skipped = stageData.IsSkipUse,
                SongId = stageData.SongNo,
                PlayTime = DateTime.ParseExact(playResultData.PlayDatetime, Constants.DATE_TIME_FORMAT, CultureInfo.InvariantCulture)
            };
            context.SongPlayData.Add(playData);
        }

        context.SaveChanges();
        var response = new PlayResultResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}