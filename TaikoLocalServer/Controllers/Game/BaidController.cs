using System.Collections.Generic;
using System.Collections;
using System.Text.Json;
using TaikoLocalServer.Services.Interfaces;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r03/chassis/baidcheck.php")]
public class BaidController : BaseController<BaidController>
{
    private readonly IUserDatumService userDatumService;

    private readonly ICardService cardService;

    private readonly ISongBestDatumService songBestDatumService;

    private readonly IDanScoreDatumService danScoreDatumService;

    public BaidController(IUserDatumService userDatumService, ICardService cardService, 
        ISongBestDatumService songBestDatumService, IDanScoreDatumService danScoreDatumService)
    {
        this.userDatumService = userDatumService;
        this.cardService = cardService;
        this.songBestDatumService = songBestDatumService;
        this.danScoreDatumService = danScoreDatumService;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetBaid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Baid request: {Request}", request.Stringify());
        BAIDResponse response;
        var card = await cardService.GetCardByAccessCode(request.AccessCode);
        if (card is null)
        {
            Logger.LogInformation("New user with access code {AccessCode}", request.AccessCode);
            var newId = cardService.GetNextBaid();

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

        var baid = card.Baid;

        var userData = await userDatumService.GetFirstUserDatumOrDefault(baid);

        var songBestData = await songBestDatumService.GetAllSongBestData(baid);
        
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
            Logger.LogError(e, "Parsing costume json data failed");
        }
        if (costumeData == null || costumeData.Count < 5)
        {
            Logger.LogWarning("Costume data is null or count less than 5!");
            costumeData = new List<uint> { 0, 0, 0, 0, 0 };
        }

        var costumeArrays = Array.Empty<uint[]>();
        try
        {
            costumeArrays = JsonSerializer.Deserialize<uint[][]>(userData.CostumeFlgArray);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing costume flg json data failed");
        }

        // The only way to get a null is provide string "null" as input,
        // which means database content need to be fixed, so better throw
        costumeArrays.ThrowIfNull("Costume flg should never be null!");

        var costumeFlg1 = new byte[Constants.COSTUME_FLAG_1_ARRAY_SIZE];
        var bitSet = new BitArray(Constants.COSTUME_FLAG_1_ARRAY_SIZE);
        foreach (var costume in costumeArrays[0])
        {
            bitSet.Set((int)costume, true);
        }
        bitSet.CopyTo(costumeFlg1, 0);

        var costumeFlg2 = new byte[Constants.COSTUME_FLAG_2_ARRAY_SIZE];
        bitSet = new BitArray(Constants.COSTUME_FLAG_2_ARRAY_SIZE);
        foreach (var costume in costumeArrays[1])
        {
            bitSet.Set((int)costume, true);
        }
        bitSet.CopyTo(costumeFlg2, 0);

        var costumeFlg3 = new byte[Constants.COSTUME_FLAG_3_ARRAY_SIZE];
        bitSet = new BitArray(Constants.COSTUME_FLAG_3_ARRAY_SIZE);
        foreach (var costume in costumeArrays[2])
        {
            bitSet.Set((int)costume, true);
        }
        bitSet.CopyTo(costumeFlg3, 0);

        var costumeFlg4 = new byte[Constants.COSTUME_FLAG_4_ARRAY_SIZE];
        bitSet = new BitArray(Constants.COSTUME_FLAG_4_ARRAY_SIZE);
        foreach (var costume in costumeArrays[3])
        {
            bitSet.Set((int)costume, true);
        }
        bitSet.CopyTo(costumeFlg4, 0);

        var costumeFlg5 = new byte[Constants.COSTUME_FLAG_5_ARRAY_SIZE];
        bitSet = new BitArray(Constants.COSTUME_FLAG_5_ARRAY_SIZE);
        foreach (var costume in costumeArrays[4])
        {
            bitSet.Set((int)costume, true);
        }
        bitSet.CopyTo(costumeFlg5, 0);

        var danData = await danScoreDatumService.GetDanScoreDatumByBaid(baid);
        
        var maxDan = danData.Where(datum => datum.ClearState != DanClearState.NotClear)
            .Select(datum => datum.DanId)
            .DefaultIfEmpty()
            .Max();
        var gotDanFlagArray = FlagCalculator.ComputeGotDanFlags(danData);

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
            ColorFace = userData.ColorFace,
            ColorBody = userData.ColorBody,
            ColorLimb = userData.ColorLimb,
            AryCostumedata = new BAIDResponse.CostumeData
            {
                Costume1 = costumeData[0],
                Costume2 = costumeData[1],
                Costume3 = costumeData[2],
                Costume4 = costumeData[3],
                Costume5 = costumeData[4]
            },
            CostumeFlg1 = costumeFlg1,
            CostumeFlg2 = costumeFlg2,
            CostumeFlg3 = costumeFlg3,
            CostumeFlg4 = costumeFlg4,
            CostumeFlg5 = costumeFlg5,
            LastPlayDatetime = userData.LastPlayDatetime.ToString(Constants.DATE_TIME_FORMAT),
            IsDispDanOn = userData.DisplayDan,
            GotDanMax = maxDan,
            GotDanFlg = gotDanFlagArray,
            GotDanextraFlg = new byte[20],
            DefaultToneSetting = userData.SelectedToneId,
            GenericInfoFlg = new byte[10],
            AryCrownCounts = crownCount,
            AryScoreRankCounts = scoreRankCount,
            IsDispAchievementOn = userData.DisplayAchievement,
            DispAchievementType = (uint)achievementDisplayDifficulty,
            IsDispAchievementTypeSet = true,
            LastPlayMode = userData.LastPlayMode,
            IsDispSouuchiOn = true,
            AiRank = 0,
            AiTotalWin = 0,
            Accesstoken = "123456",
            ContentInfo = GZipBytesUtil.GetGZipBytes(new byte[10])
        };

        return Ok(response);
    }

}