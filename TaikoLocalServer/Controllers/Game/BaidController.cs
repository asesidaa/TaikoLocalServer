using System.Text.Json;
using GameDatabase.Entities;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r03/chassis/baidcheck.php")]
public class BaidController : BaseController<BaidController>
{
    private readonly IAiDatumService aiDatumService;

    private readonly ICardService cardService;

    private readonly IDanScoreDatumService danScoreDatumService;

    private readonly ISongBestDatumService songBestDatumService;
    private readonly IUserDatumService userDatumService;

    public BaidController(IUserDatumService userDatumService, ICardService cardService,
        ISongBestDatumService songBestDatumService, IDanScoreDatumService danScoreDatumService,
        IAiDatumService aiDatumService)
    {
        this.userDatumService = userDatumService;
        this.cardService = cardService;
        this.songBestDatumService = songBestDatumService;
        this.danScoreDatumService = danScoreDatumService;
        this.aiDatumService = aiDatumService;
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
            achievementDisplayDifficulty = songBestData.Any(datum => datum.BestCrown >= CrownType.Clear)
                ? songBestData.Where(datum => datum.BestCrown >= CrownType.Clear).Max(datum => datum.Difficulty)
                : Difficulty.Easy;

        var songCountData = songBestData.Where(datum =>
            achievementDisplayDifficulty != Difficulty.UraOni
                ? datum.Difficulty == achievementDisplayDifficulty
                : datum.Difficulty is Difficulty.Oni or Difficulty.UraOni).ToList();

        var crownCount = CalculateCrownCount(songCountData);

        var scoreRankCount = CalculateScoreRankCount(songCountData);


        var costumeData = JsonHelper.GetCostumeDataFromUserData(userData, Logger);

        var costumeArrays = JsonHelper.GetCostumeUnlockDataFromUserData(userData, Logger);

        var costumeFlagArrays = Constants.CostumeFlagArraySizes
            .Select((size, index) => FlagCalculator.GetBitArrayFromIds(costumeArrays[index], size, Logger))
            .ToList();

        var danData = await danScoreDatumService.GetDanScoreDatumByBaid(baid);

        var maxDan = danData.Where(datum => datum.ClearState != DanClearState.NotClear)
            .Select(datum => datum.DanId)
            .DefaultIfEmpty()
            .Max();
        var gotDanFlagArray = FlagCalculator.ComputeGotDanFlags(danData);

        var genericInfoFlg = Array.Empty<uint>();
        try
        {
            genericInfoFlg = JsonSerializer.Deserialize<uint[]>(userData.GenericInfoFlgArray);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing genericinfo flg json data failed");
        }

        // The only way to get a null is provide string "null" as input,
        // which means database content need to be fixed, so better throw
        genericInfoFlg.ThrowIfNull("Genericinfo flg should never be null!");

        var genericInfoFlgLength = genericInfoFlg.Any() ? genericInfoFlg.Max() + 1 : 0;
        var genericInfoFlgArray = FlagCalculator.GetBitArrayFromIds(genericInfoFlg, (int)genericInfoFlgLength, Logger);

        var aiRank = (uint)(userData.AiWinCount / 10);
        if (aiRank > 11) aiRank = 11;
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
            CostumeFlg1 = costumeFlagArrays[0],
            CostumeFlg2 = costumeFlagArrays[1],
            CostumeFlg3 = costumeFlagArrays[2],
            CostumeFlg4 = costumeFlagArrays[3],
            CostumeFlg5 = costumeFlagArrays[4],
            LastPlayDatetime = userData.LastPlayDatetime.ToString(Constants.DATE_TIME_FORMAT),
            IsDispDanOn = userData.DisplayDan,
            GotDanMax = maxDan,
            GotDanFlg = gotDanFlagArray,
            GotDanextraFlg = new byte[20],
            DefaultToneSetting = userData.SelectedToneId,
            GenericInfoFlg = genericInfoFlgArray,
            AryCrownCounts = crownCount,
            AryScoreRankCounts = scoreRankCount,
            IsDispAchievementOn = userData.DisplayAchievement,
            DispAchievementType = (uint)achievementDisplayDifficulty,
            IsDispAchievementTypeSet = true,
            LastPlayMode = userData.LastPlayMode,
            IsDispSouuchiOn = true,
            AiRank = aiRank,
            AiTotalWin = (uint)userData.AiWinCount,
            Accesstoken = "123456",
            ContentInfo = GZipBytesUtil.GetGZipBytes(new byte[10])
        };

        return Ok(response);
    }

    private static uint[] CalculateScoreRankCount(IReadOnlyCollection<SongBestDatum> songCountData)
    {
        var scoreRankCount = new uint[7];
        foreach (var scoreRankType in Enum.GetValues<ScoreRank>())
            if (scoreRankType != ScoreRank.None)
                scoreRankCount[(int)scoreRankType - 2] =
                    (uint)songCountData.Count(datum => datum.BestScoreRank == scoreRankType);

        return scoreRankCount;
    }

    private static uint[] CalculateCrownCount(IReadOnlyCollection<SongBestDatum> songCountData)
    {
        var crownCount = new uint[3];
        foreach (var crownType in Enum.GetValues<CrownType>())
            if (crownType != CrownType.None)
                crownCount[(int)crownType - 1] = (uint)songCountData.Count(datum => datum.BestCrown == crownType);

        return crownCount;
    }
}