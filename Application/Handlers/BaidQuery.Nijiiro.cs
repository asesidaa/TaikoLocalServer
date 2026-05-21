using Throw;

namespace TaikoLocalServer.Application.Handlers;

public partial class BaidQueryHandler
{
    private partial async ValueTask<CommonBaidResponse> HandleNijiiro(BaidQuery request, CancellationToken cancellationToken)
    {
        var card = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
        if (card is null)
        {
            logger.LogInformation("New user with access code {AccessCode}", request.AccessCode);
            var nextBaid = await context.Cards
                .Select(c => (uint?)c.Baid)
                .MaxAsync(cancellationToken) ?? 0;
            return new CommonBaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = nextBaid + 1
            };
        }

        var baid = card.Baid;
        var saveData = await context.UserSaveDataNijiiro.FindAsync([baid], cancellationToken);
        if (saveData is null)
        {
            return new CommonBaidResponse
            {
                Result = 1,
                IsNewUser = true,
                Baid = baid
            };
        }

        var userData = await context.UserData.FindAsync(baid, cancellationToken);
        userData.ThrowIfNull($"User not found for card with Baid {baid}!");

        var timeLimitSongsList = gameDataService.Nijiiro().GetTimeLimitedSongsList();

        var songBestData = await context.SongBestDataNijiiro
            .Where(datum => datum.Baid == baid)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var achievementDisplayDifficulty = saveData.AchievementDisplayDifficulty;
        var isDispAchievementTypeSet = true;
        if (achievementDisplayDifficulty == Difficulty.None)
        {
            isDispAchievementTypeSet = false;
            achievementDisplayDifficulty = songBestData
                .Where(datum => datum.BestCrown >= CrownType.Clear)
                .Select(datum => datum.Difficulty)
                .DefaultIfEmpty(Difficulty.Easy)
                .Max();
        }
        // For each crown type, calculate how many songs have that crown type
        var crownCountData = songBestData
            .Where(datum => !timeLimitSongsList.Contains(datum.SongId) && (datum.Difficulty == achievementDisplayDifficulty || (achievementDisplayDifficulty == Difficulty.UraOni && datum.Difficulty == Difficulty.Oni)))
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
            .Where(datum => !timeLimitSongsList.Contains(datum.SongId) && (datum.Difficulty == achievementDisplayDifficulty || (achievementDisplayDifficulty == Difficulty.UraOni && datum.Difficulty == Difficulty.Oni)))
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
        
        List<uint> costumeData = [saveData.CurrentKigurumi, saveData.CurrentHead, saveData.CurrentBody, saveData.CurrentFace, saveData.CurrentPuchi];
        
        List<List<uint>> costumeArrays = 
            [saveData.UnlockedKigurumi, saveData.UnlockedHead, saveData.UnlockedBody, saveData.UnlockedFace, saveData.UnlockedPuchi];

        var costumeFlagArrays = gameDataService.Nijiiro().GetCostumeFlagArraySizes()
            .Select((size, index) => FlagCalculator.GetBitArrayFromIds(costumeArrays[index], size, logger))
            .ToList();

        var allDans = await context.DanScoreDataNijiiro
            .Where(datum => datum.Baid == baid && (datum.DanType == DanType.Normal || datum.DanType == DanType.Gaiden))
            .Include(datum => datum.DanStageScoreData)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var danData = allDans.Where(datum => datum.DanType == DanType.Normal).ToList();
        var gaidenData = allDans.Where(datum => datum.DanType == DanType.Gaiden).ToList();
        
        var maxDan = danData.Where(datum => datum.ClearState != DanClearState.NotClear)
            .Select(datum => datum.DanId)
            .DefaultIfEmpty()
            .Max();
        
        var danDataDictionary = gameDataService.Nijiiro().GetCommonDanDataDictionary();
        var danIdList = danDataDictionary.Keys.ToList();
        var gotDanFlagArray = FlagCalculator.ComputeGotDanFlags(danData, danIdList);
        
        var gaidenDataDictionary = gameDataService.Nijiiro().GetCommonGaidenDataDictionary();
        var gaidenIdList = gaidenDataDictionary.Keys.ToList();
        var gotGaidenFlagArray = FlagCalculator.ComputeGotDanFlags(gaidenData, gaidenIdList);

        var genericInfoFlg = saveData.GenericInfoFlgArray;

        var genericInfoFlgLength = genericInfoFlg.Any() ? genericInfoFlg.Max() + 1 : 0;
        var genericInfoFlgArray = FlagCalculator.GetBitArrayFromIds(genericInfoFlg, (int)genericInfoFlgLength, logger);

        var aiRank = (uint)(saveData.AiWinCount / 10);
        if (aiRank > 10)
        {
            aiRank = 10;
        }

        return new CommonBaidResponse
        {
            Result = 1,
            IsNewUser = false,
            Baid = baid,
            MyDonName = userData.MyDonName,
            MyDonNameLanguage = userData.MyDonNameLanguage,
            AryCrownCounts = crownCount,
            AryScoreRankCounts = scoreRankCount,
            ColorBody = saveData.ColorBody,
            ColorFace = saveData.ColorFace,
            ColorLimb = saveData.ColorLimb,
            CostumeData = costumeData,
            CostumeFlagArrays = costumeFlagArrays,
            DisplayDan = saveData.DisplayDan,
            IsDispSouuchiOn = saveData.DisplaySouUchi,
            DispAchievementType = (uint)achievementDisplayDifficulty,
            IsDispAchievementTypeSet = isDispAchievementTypeSet,
            GenericInfoFlg = genericInfoFlgArray,
            GotDanFlg = gotDanFlagArray,
            GotDanMax = maxDan,
            GotGaidenFlg = gotGaidenFlagArray,
            IsDispAchievementOn = saveData.DisplayAchievement,
            LastPlayDatetime = saveData.LastPlayDatetime.ToString(Constants.DateTimeFormat),
            LastPlayMode = saveData.LastPlayMode,
            SelectedToneId = saveData.SelectedToneId,
            Title = saveData.Title,
            TitlePlateId = saveData.TitlePlateId,
            AiTotalWin = (uint)saveData.AiWinCount,
            AiRank = aiRank
        };
    }
}
