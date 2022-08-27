using System.Globalization;
using System.Text.Json;
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

        var lastPlayDatetime = DateTime.ParseExact(playResultData.PlayDatetime, Constants.DATE_TIME_FORMAT, CultureInfo.InvariantCulture);
        userdata.LastPlayDatetime = lastPlayDatetime;
        context.UserData.Update(userdata);

        var bestData = context.SongBestData.Where(datum => datum.Baid == request.BaidConf).ToList();
        foreach (var stageData in playResultData.AryStageInfoes)
        {
            var insert = false;
            var bestDatum = bestData
                .FirstOrDefault(datum => datum.SongId == stageData.SongNo &&
                                         datum.Difficulty == (Difficulty)stageData.Level);
            var crown = CrownType.None;
            if (stageData.PlayResult > 0)
            {
                crown = CrownType.Clear;
                if (stageData.NgCnt == 0)
                {
                    crown = CrownType.Gold;
                    if (stageData.OkCnt == 0)
                    {
                        crown = CrownType.Dondaful;
                    }
                }
            }
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
            if (bestDatum.BestCrown < crown)
            {
                bestDatum.BestCrown = crown;
            }
            if ((uint)bestDatum.BestScoreRank < stageData.ScoreRank)
            {
                bestDatum.BestScoreRank = (ScoreRank)stageData.ScoreRank;
            }
            if (bestDatum.BestScore < stageData.PlayScore)
            {
                bestDatum.BestScore = stageData.PlayScore;
            }
            if (bestDatum.BestRate < stageData.ScoreRate)
            {
                bestDatum.BestRate = stageData.ScoreRate;
            }

            if (insert)
            {
                context.SongBestData.Add(bestDatum);
            }
            else
            {
                context.SongBestData.Update(bestDatum);
            }
            
            var playData = new SongPlayDatum
            {
                Baid = request.BaidConf,
                GoodCount = stageData.GoodCnt,
                OkCount = stageData.OkCnt,
                MissCount = stageData.NgCnt,
                ComboCount = stageData.ComboCnt,
                HitCount = stageData.HitCnt,
                Crown = crown,
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

        context.SaveChanges();
        var response = new PlayResultResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}