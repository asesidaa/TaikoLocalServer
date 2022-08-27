using System.Text.Json;
using TaikoLocalServer.Common;
using TaikoLocalServer.Common.Enums;
using TaikoLocalServer.Context;
using TaikoLocalServer.Entities;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/baidcheck.php")]
public class BaidController:ControllerBase
{
    private readonly ILogger<BaidController> logger;

    private readonly TaikoDbContext context;

    public BaidController(ILogger<BaidController> logger, TaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetBaid([FromBody] BAIDRequest request)
    {
        logger.LogInformation("Baid request: {Request}", request.Stringify());
        BAIDResponse response;
        if (!context.Cards.Any(card => card.AccessCode == request.AccessCode))
        {
            logger.LogInformation("New user from access code {AccessCode}", request.AccessCode);
            var newId = context.Cards.Any() ? context.Cards.Max(card => card.Baid) + 1 : 1;

            response = new BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                ComSvrResult = 1,
                MbId = 1,
                Baid = newId,
                AccessCode = request.AccessCode,
                IsPublish = true,
                CardOwnNum = 1,
                RegCountryId = "JPN",
                PurposeId = 1,
                RegionId = 1
            };

            return Ok(response);
        }

        var baid = context.Cards.First(card => card.AccessCode == request.AccessCode).Baid;

        var userData = new UserDatum();
        if (context.UserData.Any(datum => datum.Baid == baid))
        {
            userData = context.UserData.First(datum => datum.Baid == baid);
        }

        var songBestData = context.SongBestData.Where(datum => datum.Baid == baid).ToList();
        
        var achievementDisplayDifficulty = userData.AchievementDisplayDifficulty;
        if (userData.AchievementDisplayDifficulty == Difficulty.None)
        {
            achievementDisplayDifficulty = songBestData.Any(datum => datum.BestCrown >= CrownType.Clear) ? 
                songBestData.Where(datum => datum.BestCrown >= CrownType.Clear).Max(datum => datum.Difficulty) : 
                Difficulty.Easy;
        }

        var songCountData = songBestData.Where(datum => achievementDisplayDifficulty != Difficulty.UraOni ?
                                                   datum.Difficulty == achievementDisplayDifficulty : 
                                                   datum.Difficulty is Difficulty.Oni or Difficulty.UraOni).ToList();

        var crownCount = new uint[3];
        foreach (var crownType in Enum.GetValues<CrownType>())
        {
            if (crownType != CrownType.None)
            {
                crownCount[(int)crownType - 1] = (uint)songCountData.Count(datum => datum.BestCrown == crownType);
            }
        }

        var scoreRankCount = new uint[7];
        foreach (var scoreRankType in Enum.GetValues<ScoreRank>())
        {
            if (scoreRankType != ScoreRank.None)
            {
                scoreRankCount[(int)scoreRankType - 2] = (uint)songCountData.Count(datum => datum.BestScoreRank == scoreRankType);
            }
        }


        var costumeData = new List<uint>{ 0, 0, 0, 0, 0 };
        try
        {
            costumeData = JsonSerializer.Deserialize<List<uint>>(userData.CostumeData);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parsing costume json data failed");
        }
        if (costumeData == null || costumeData.Count < 5)
        {
            logger.LogWarning("Costume data is null or count less than 5!");
            costumeData = new List<uint> { 0, 0, 0, 0, 0 };
        }

        var costumeFlag = new byte[10];
        Array.Fill(costumeFlag, byte.MaxValue);

        response = new BAIDResponse
        {
            Result = 1,
            PlayerType = 0,
            ComSvrResult = 1,
            MbId = 1,
            Baid = baid,
            AccessCode = request.AccessCode,
            IsPublish = true,
            CardOwnNum = 1,
            RegCountryId = "JPN",
            PurposeId = 1,
            RegionId = 1,
            MydonName = userData.MyDonName,
            Title = userData.Title,
            TitleplateId = userData.TitlePlateId,
            ColorFace = 0,
            ColorBody = 1,
            ColorLimb = 3,
            AryCostumedata = new BAIDResponse.CostumeData
            {
                Costume1 = costumeData[0],
                Costume2 = costumeData[1],
                Costume3 = costumeData[2],
                Costume4 = costumeData[3],
                Costume5 = costumeData[4]
            },
            CostumeFlg1 = costumeFlag,
            CostumeFlg2 = costumeFlag,
            CostumeFlg3 = costumeFlag,
            CostumeFlg4 = costumeFlag,
            CostumeFlg5 = costumeFlag,
            LastPlayDatetime = userData.LastPlayDatetime.ToString(Constants.DATE_TIME_FORMAT),
            IsDispDanOn = userData.DisplayDan,
            GotDanMax = 1,
            GotDanFlg = new byte[20],
            GotDanextraFlg = new byte[20],
            DefaultToneSetting = 0,
            GenericInfoFlg = new byte[10],
            AryCrownCounts = crownCount,
            AryScoreRankCounts = scoreRankCount,
            IsDispAchievementOn = userData.DisplayAchievement,
            DispAchievementType = (uint)achievementDisplayDifficulty,
            IsDispAchievementTypeSet = true,
            LastPlayMode = 0,
            IsDispSouuchiOn = true,
            AiRank = 0,
            AiTotalWin = 0,
            Accesstoken = "123456",
            ContentInfo = GZipBytesUtil.GetGZipBytes(new byte[10])
        };

        return Ok(response);
    }

}