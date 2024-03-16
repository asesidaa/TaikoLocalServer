using GameDatabase.Context;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record BaidQuery(string AccessCode) : IRequest<CommonBaidResponse>;

public class BaidQueryHandler(
    TaikoDbContext context,
    ILogger<BaidQueryHandler> logger,
    IGameDataService gameDataService)
    : IRequestHandler<BaidQuery, CommonBaidResponse>
{
    public async Task<CommonBaidResponse> Handle(BaidQuery request, CancellationToken cancellationToken)
    {
        var card = await context.Cards.FindAsync(request.AccessCode);
        if (card is null)
        {
            logger.LogInformation("New user with access code {AccessCode}", request.AccessCode);
            return new CommonBaidResponse
            {
                IsNewUser = true,
                Baid = context.Cards.Any() ? context.Cards.AsEnumerable().Max(c => c.Baid) + 1 : 1
            };
        }

        var baid = card.Baid;
        var userData = await context.UserData.FindAsync(baid, cancellationToken);
        userData.ThrowIfNull($"User not found for card with Baid {baid}!");

        var songBestData = context.SongBestData.Where(datum => datum.Baid == baid).ToList();
        var achievementDisplayDifficulty = userData.AchievementDisplayDifficulty;
        if (achievementDisplayDifficulty == Difficulty.None)
        {
            achievementDisplayDifficulty = songBestData
                .Where(datum => datum.BestCrown >= CrownType.Clear)
                .Select(datum => datum.Difficulty)
                .DefaultIfEmpty(Difficulty.Easy)
                .Max();
        }
        // For each crown type, calculate how many songs have that crown type
        var crownCountData = songBestData
            .Where(datum => datum.BestCrown >= CrownType.Clear)
            .GroupBy(datum => datum.BestCrown)
            .ToDictionary(datums => datums.Key, datums => (uint)datums.Count());
        var crownCount = new uint[3];
        foreach (var crownType in Enum.GetValues<CrownType>())
        {
            if (crownType != CrownType.None)
            {
                crownCount[(int)crownType - 1] = crownCountData.GetValueOrDefault(crownType, (uint)0);
            }
        }
        
        var scoreRankData = songBestData
            .Where(datum => datum.BestCrown >= CrownType.Clear)
            .GroupBy(datum => datum.BestScoreRank)
            .ToDictionary(datums => datums.Key, datums => (uint)datums.Count());
        var scoreRankCount = new uint[7];
        foreach (var scoreRank in Enum.GetValues<ScoreRank>())
        {
            if (scoreRank != ScoreRank.None)
            {
                scoreRankCount[(int)scoreRank - 2] = scoreRankData.GetValueOrDefault(scoreRank, (uint)0);
            }
        }

        var costumeData = JsonHelper.GetCostumeDataFromUserData(userData, logger);

        List<List<uint>> costumeArrays = 
            [userData.UnlockedKigurumi, userData.UnlockedHead, userData.UnlockedBody, userData.UnlockedFace, userData.UnlockedPuchi];

        var costumeFlagArrays = gameDataService.GetCostumeFlagArraySizes()
            .Select((size, index) => FlagCalculator.GetBitArrayFromIds(costumeArrays[index], size, logger))
            .ToList();

        var danData = await context.DanScoreData
            .Where(datum => datum.Baid == baid && datum.DanType == DanType.Normal)
            .Include(datum => datum.DanStageScoreData).ToListAsync(cancellationToken);
        var gaidenData = await context.DanScoreData
            .Where(datum => datum.Baid == baid && datum.DanType == DanType.Gaiden)
            .Include(datum => datum.DanStageScoreData).ToListAsync(cancellationToken);
        
        var maxDan = danData.Where(datum => datum.ClearState != DanClearState.NotClear)
            .Select(datum => datum.DanId)
            .DefaultIfEmpty()
            .Max();
        
        var danDataDictionary = gameDataService.GetCommonDanDataDictionary();
        var danIdList = danDataDictionary.Keys.ToList();
        var gotDanFlagArray = FlagCalculator.ComputeGotDanFlags(danData, danIdList);
        
        var gaidenDataDictionary = gameDataService.GetCommonGaidenDataDictionary();
        var gaidenIdList = gaidenDataDictionary.Keys.ToList();
        var gotGaidenFlagArray = FlagCalculator.ComputeGotDanFlags(gaidenData, gaidenIdList);

        var genericInfoFlg = userData.GenericInfoFlgArray;

        var genericInfoFlgLength = genericInfoFlg.Any() ? genericInfoFlg.Max() + 1 : 0;
        var genericInfoFlgArray = FlagCalculator.GetBitArrayFromIds(genericInfoFlg, (int)genericInfoFlgLength, logger);

        var aiRank = (uint)(userData.AiWinCount / 10);
        if (aiRank > 11)
        {
            aiRank = 11;
        }

        return new CommonBaidResponse
        {
            IsNewUser = false,
            Baid = baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            AryCrownCounts = crownCount,
            AryScoreRankCounts = scoreRankCount,
            ColorBody = userData.ColorBody,
            ColorFace = userData.ColorFace,
            ColorLimb = userData.ColorLimb,
            CostumeData = costumeData,
            CostumeFlagArrays = costumeFlagArrays,
            DisplayDan = userData.DisplayDan,
            DispAchievementType = (uint)achievementDisplayDifficulty,
            GenericInfoFlg = genericInfoFlgArray,
            GotDanFlg = gotDanFlagArray,
            GotDanMax = maxDan,
            GotGaidenFlg = gotGaidenFlagArray,
            IsDispAchievementOn = userData.DisplayAchievement,
            LastPlayDatetime = userData.LastPlayDatetime.ToString(Constants.DATE_TIME_FORMAT),
            LastPlayMode = userData.LastPlayMode,
            SelectedToneId = userData.SelectedToneId,
            Title = userData.Title,
            TitlePlateId = userData.TitlePlateId
        };
    }
}
