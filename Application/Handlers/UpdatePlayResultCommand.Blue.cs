using System.Globalization;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint MinBlueCourseLevel = 1;
    private const uint MaxBlueCourseLevel = 5;
    private const uint BlueDanCostumeId = 36;

    private partial async ValueTask<uint> HandleBlue(
        UpdatePlayResultCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Baid == 0)
        {
            return 1;
        }

        var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Game uploading a non existing Blue user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        if (playResultData.IsTokkunPlayResult)
        {
            return await HandleBlueTokkun(request.Baid, playResultData, cancellationToken);
        }

        if (playResultData.IsBattlePlayResult)
        {
            return await HandleBlueBattle(request.Baid, playResultData, cancellationToken);
        }

        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var blue = gameDataService.Blue();
        var shopSeasonState = await context.GetOrCreateActiveBlueShopSeasonStateAsync(
            saveData,
            blue.ItemShopCatalog,
            cancellationToken);
        var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;

        if (!CanAddBlue(currentDonmedal, playResultData.GetDonmedal)
            || !CanAddBlue(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal))
        {
            logger.LogWarning("Rejecting invalid Blue medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        var playTime = ParseBluePlayDatetimeOrNow(playResultData.PlayDatetime);

        if (shopSeasonState is null)
        {
            saveData.TotalGetDonmedal += playResultData.GetDonmedal;
        }
        else
        {
            shopSeasonState.TotalGetDonmedal += playResultData.GetDonmedal;
            shopSeasonState.UpdatedAt = DateTime.UtcNow;
        }

        saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;
        saveData.ItemshopTutorialFlg = playResultData.ItemshopTutorialFlg ?? saveData.ItemshopTutorialFlg;
        saveData.IsDevil = playResultData.IsDevil ?? saveData.IsDevil;
        saveData.IsExplain = playResultData.IsExplain ?? saveData.IsExplain;
        saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;
        if (playResultData.HasDifficultyPlayedCourse)
        {
            saveData.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
        }

        if (playResultData.HasDifficultyPlayedStar)
        {
            saveData.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
        }

        saveData.LastPlayDatetime = playTime;
        saveData.PrevAreaCode = playResultData.AreaCode;

        if (playResultData.HasAryCurrentCostume && saveData.IsAutoCostumeOn)
        {
            ApplyCostume(saveData, playResultData.AryCurrentCostume);
        }

        ApplyUnlockBits(saveData, playResultData);

        foreach (var stage in playResultData.AryStageInfoes)
        {
            if (!IsSupportedBlueStage(request.Baid, stage))
            {
                continue;
            }

            Ac15ProfileCounterUpdater.ApplyBlueStage(saveData, stage);
        }

        await Ac15DaniService.SaveAsync(
            playResultData,
            Ac15EraProfiles.Blue,
            blue.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new BlueAc15DaniAdapter(context, blue.TaikojukuFileOrder.Select(row => row.ChallengeLevel)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, BlueDanCostumeId),
            update =>
            {
                saveData.GotDanFlg = update.GotDanFlg;
                saveData.GotDanExtraFlg = update.GotDanExtraFlg;
                saveData.GotDanMax = update.GotDanMax;
                saveData.DispTaikojukuDan = update.DisplayDan;
                if (update.ApplyDanCostume)
                {
                    saveData.Costume1 = update.DanCostumeId;
                    saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, [update.DanCostumeId], BlueProtocolBytes.CostumeFlagBytes);
                }
            },
            logger,
            cancellationToken);

        return await Ac15NormalPlayService.SaveAsync(
            request.Baid,
            playResultData,
            Ac15EraProfiles.Blue,
            new BlueAc15NormalPlayAdapter(context),
            DefaultAc15EraHooks.Instance,
            cancellationToken);
    }

    private bool IsSupportedBlueStage(uint baid, CommonPlayResultData.StageData stage)
    {
        if (!BluePlayResultMapping.IsSupportedNormalStageMode(stage.StageMode))
        {
            logger.LogWarning(
                "Skipping unsupported Blue stage mode for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        if (stage.SongNo >= BlueProtocolBytes.SongFlagBytes * 8 || stage.Level is < MinBlueCourseLevel or > MaxBlueCourseLevel)
        {
            logger.LogWarning(
                "Skipping invalid Blue stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        return true;
    }

    private static void ApplyCostume(UserSaveDataBlue saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, [costume.Costume1], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBlueBits(saveData.CostumeFlg2, [costume.Costume2], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBlueBits(saveData.CostumeFlg3, [costume.Costume3], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBlueBits(saveData.CostumeFlg4, [costume.Costume4], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBlueBits(saveData.CostumeFlg5, [costume.Costume5], BlueProtocolBytes.CostumeFlagBytes);
    }

    private static void ApplyUnlockBits(UserSaveDataBlue saveData, CommonPlayResultData playResultData)
    {
        saveData.ReleaseSongFlg = SetBlueBits(saveData.ReleaseSongFlg, playResultData.ReleaseSongNoes, BlueProtocolBytes.SongFlagBytes);
        saveData.ToneFlg = SetBlueBits(saveData.ToneFlg, playResultData.GetToneNoes, BlueProtocolBytes.ToneFlagBytes);
        saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBlueBits(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBlueBits(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBlueBits(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBlueBits(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.TitleFlg = SetBlueBits(saveData.TitleFlg, playResultData.GetTitleNoes, BlueProtocolBytes.TitleFlagBytes);
    }

    private static bool CanAddBlue(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static DateTime ParseBluePlayDatetimeOrNow(string playDatetime)
    {
        var formats = new[] { Constants.DateTimeFormat, "yyyy-MM-dd HH:mm:ss" };
        return DateTime.TryParseExact(
            playDatetime,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : DateTime.Now;
    }

    private static byte[] SetBlueBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = BlueProtocolBytes.FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }
}
