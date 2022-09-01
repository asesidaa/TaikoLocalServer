using System.Globalization;
using System.Text.Json;

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

        var response = new PlayResultResponse
        {
            Result = 1
        };   
       
        var lastPlayDatetime = DateTime.ParseExact(playResultData.PlayDatetime, Constants.DATE_TIME_FORMAT, CultureInfo.InvariantCulture);
        
        UpdateUserData(request, playResultData, lastPlayDatetime);
        var playMode = (PlayMode)playResultData.PlayMode;
        
        if (playMode == PlayMode.DanMode)
        {
            UpdateDanPlayData(request, playResultData);
            context.SaveChanges();
            return Ok(response);
        }
        
        var bestData = context.SongBestData.Where(datum => datum.Baid == request.BaidConf).ToList();
        for (var songNumber = 0; songNumber < playResultData.AryStageInfoes.Count; songNumber++)
        {
            var stageData = playResultData.AryStageInfoes[songNumber];
            
            UpdateBestData(request, stageData, bestData);

            UpdatePlayData(request, songNumber, stageData, lastPlayDatetime);
        }

        context.SaveChanges();
        return Ok(response);
    }

    private void UpdateDanPlayData(PlayResultRequest request, PlayResultDataRequest playResultDataRequest)
    {
        if (playResultDataRequest.IsNotRecordedDan)
        {
            return;
        }
        var danScoreDataQuery = context.DanScoreData
             .Where(datum => datum.Baid == request.BaidConf &&
                             datum.DanId == playResultDataRequest.DanId)
             .Include(datum => datum.DanStageScoreData);
        var danScoreData = new DanScoreDatum
        {
            Baid = request.BaidConf,
            DanId = playResultDataRequest.DanId
        };
        var insert = true;
        if (danScoreDataQuery.Any())
        {
            danScoreData = danScoreDataQuery.First();
            insert = false;
        }
        danScoreData.ClearState = (DanClearState)playResultDataRequest.DanResult;
        danScoreData.ArrivalSongCount = (uint)playResultDataRequest.AryStageInfoes.Count;
        danScoreData.ComboCountTotal = playResultDataRequest.ComboCntTotal;
        danScoreData.SoulGaugeTotal = playResultDataRequest.SoulGaugeTotal;

        UpdateDanStageData(playResultDataRequest, danScoreData);

        if (insert)
        {
            context.DanScoreData.Add(danScoreData);
            return;
        }
        context.DanScoreData.Update(danScoreData);
    }
    private void UpdateDanStageData(PlayResultDataRequest playResultDataRequest, DanScoreDatum danScoreData)
    {
        for (var songNumber = 0; songNumber < playResultDataRequest.AryStageInfoes.Count; songNumber++)
        {
            var stageData = playResultDataRequest.AryStageInfoes[songNumber];
            var add = true;

            var danStageData = new DanStageScoreDatum
            {
                Baid = danScoreData.Baid,
                DanId = danScoreData.DanId,
                SongNumber = (uint)songNumber
            };
            if (danScoreData.DanStageScoreData.Any(datum => datum.SongNumber == songNumber))
            {
                danStageData = danScoreData.DanStageScoreData.First(datum => datum.SongNumber == songNumber);
                add = false;
            }

            if (danStageData.HighScore >= stageData.PlayScore)
            {
                continue;
            }
            danStageData.GoodCount = stageData.GoodCnt;
            danStageData.OkCount = stageData.OkCnt;
            danStageData.BadCount = stageData.NgCnt;
            danStageData.PlayScore = stageData.PlayScore;
            danStageData.HighScore = stageData.PlayScore;
            danStageData.ComboCount = stageData.ComboCnt;
            danStageData.TotalHitCount = stageData.HitCnt;
            danStageData.DrumrollCount = stageData.PoundCnt;

            if (add)
            {
                context.DanStageScoreData.Add(danStageData);
                // danScoreData.DanStageScoreData.Add(danStageData);
                continue;
            }

            context.DanStageScoreData.Update(danStageData);
            // danScoreData.DanStageScoreData[songNumber] = danStageData;
        }
    }

    private void UpdatePlayData(PlayResultRequest request, int songNumber, PlayResultDataRequest.StageData stageData, DateTime lastPlayDatetime)
    {
        var playData = new SongPlayDatum
        {
            Baid = request.BaidConf,
            SongNumber = (uint)songNumber,
            GoodCount = stageData.GoodCnt,
            OkCount = stageData.OkCnt,
            MissCount = stageData.NgCnt,
            ComboCount = stageData.ComboCnt,
            HitCount = stageData.HitCnt,
            Crown = PlayResultToCrown(stageData),
            Score = stageData.PlayScore,
            ScoreRate = stageData.ScoreRate,
            ScoreRank = (ScoreRank)stageData.ScoreRank,
            Skipped = stageData.IsSkipUse,
            SongId = stageData.SongNo,
            PlayTime = lastPlayDatetime,
            Difficulty = (Difficulty)stageData.Level
        };
        context.SongPlayData.Add(playData);
    }
    private void UpdateUserData(PlayResultRequest request, PlayResultDataRequest playResultData, DateTime lastPlayDatetime)
    {
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

        userdata.LastPlayDatetime = lastPlayDatetime;
        context.UserData.Update(userdata);
    }

    private void UpdateBestData(PlayResultRequest request, PlayResultDataRequest.StageData stageData, IEnumerable<SongBestDatum> bestData)
    {
        var insert = false;
        var data = stageData;
        var bestDatum = bestData
            .FirstOrDefault(datum => datum.SongId == data.SongNo &&
                                     datum.Difficulty == (Difficulty)data.Level);

        // Determine whether it is dondaful crown as this is not reflected by play result
        var crown = PlayResultToCrown(stageData);

        if (bestDatum is null)
        {
            insert = true;
            bestDatum = new SongBestDatum
            {
                Baid = request.BaidConf,
                SongId = stageData.SongNo,
                Difficulty = (Difficulty)stageData.Level
            };
        }

        bestDatum.UpdateBestData(crown, stageData.ScoreRank, stageData.PlayScore, stageData.ScoreRate);

        if (insert)
        {
            context.SongBestData.Add(bestDatum);
        }
        else
        {
            context.SongBestData.Update(bestDatum);
        }
    }
    private static CrownType PlayResultToCrown(PlayResultDataRequest.StageData stageData)
    {
        var crown = (CrownType)stageData.PlayResult;
        if (crown == CrownType.Gold && stageData.OkCnt == 0)
        {
            crown = CrownType.Dondaful;
        }
        return crown;
    }
}