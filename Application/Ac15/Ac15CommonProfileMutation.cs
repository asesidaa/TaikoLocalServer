namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CommonProfileMutation
{
    public static bool TryApply<TSave>(
        TSave saveData,
        IAc15ShopSeasonState? shopSeasonState,
        CommonPlayResultData playResultData,
        IReadOnlyList<CommonPlayResultData.StageData> countedStages,
        Ac15ProfileCounterAccess<TSave> counterAccess,
        Ac15UnlockFlagAccess<TSave> unlockAccess,
        Ac15ProtocolLimits limits,
        DateTime playTime)
        where TSave :
            IAc15MedalSaveData,
            IAc15TutorialSaveData,
            IAc15PlayProfileSaveData,
            IAc15CustomizationSaveData
    {
        var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;
        if (!CanAdd(currentDonmedal, playResultData.GetDonmedal)
            || !CanAdd(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal))
        {
            return false;
        }

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
        saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;

        ApplyShared(
            saveData,
            playResultData,
            countedStages,
            counterAccess,
            unlockAccess,
            limits,
            playTime);

        return true;
    }

    public static bool TryApplyDonPoints<TSave>(
        TSave saveData,
        CommonPlayResultData playResultData,
        IReadOnlyList<CommonPlayResultData.StageData> countedStages,
        Ac15ProfileCounterAccess<TSave> counterAccess,
        Ac15UnlockFlagAccess<TSave> unlockAccess,
        Ac15ProtocolLimits limits,
        DateTime playTime)
        where TSave :
            IAc15DonPointSaveData,
            IAc15PlayTutorialSaveData,
            IAc15PlayProfileSaveData,
            IAc15CustomizationSaveData
    {
        if (!CanAdd(saveData.TotalGetDonpoint, playResultData.GetDonpoint))
        {
            return false;
        }

        saveData.TotalGetDonpoint += playResultData.GetDonpoint;
        saveData.RewardPtn = playResultData.RewardPtn ?? saveData.RewardPtn;
        saveData.RewardProgress = playResultData.RewardProgress ?? saveData.RewardProgress;
        saveData.DifficultyTutorialFlg = playResultData.DifficultyTutorialFlg ?? saveData.DifficultyTutorialFlg;

        ApplyShared(
            saveData,
            playResultData,
            countedStages,
            counterAccess,
            unlockAccess,
            limits,
            playTime);

        return true;
    }

    private static void ApplyShared<TSave>(
        TSave saveData,
        CommonPlayResultData playResultData,
        IReadOnlyList<CommonPlayResultData.StageData> countedStages,
        Ac15ProfileCounterAccess<TSave> counterAccess,
        Ac15UnlockFlagAccess<TSave> unlockAccess,
        Ac15ProtocolLimits limits,
        DateTime playTime)
        where TSave :
            IAc15PlayTutorialSaveData,
            IAc15PlayProfileSaveData,
            IAc15CustomizationSaveData
    {
        saveData.IsDevil = playResultData.IsDevil ?? saveData.IsDevil;
        saveData.IsExplain = playResultData.IsExplain ?? saveData.IsExplain;
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
            Ac15CustomizationMutation.ApplyCurrentCostume(saveData, playResultData.AryCurrentCostume, limits);
        }

        unlockAccess.ReleaseSongs?.Invoke(saveData, playResultData.ReleaseSongNoes.Where(id => id < (uint)limits.SongFlagBytes * 8));
        unlockAccess.Tones(saveData, playResultData.GetToneNoes.Where(id => id < (uint)limits.ToneFlagBytes * 8));
        unlockAccess.Titles(saveData, playResultData.GetTitleNoes.Where(id => id < (uint)limits.TitleFlagBytes * 8));
        unlockAccess.Costume1(saveData, playResultData.GetCostumeNo1s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume2(saveData, playResultData.GetCostumeNo2s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume3(saveData, playResultData.GetCostumeNo3s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume4(saveData, playResultData.GetCostumeNo4s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume5(saveData, playResultData.GetCostumeNo5s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));

        foreach (var stage in countedStages)
        {
            Ac15ProfileCounterUpdater.ApplyStage(saveData, stage, counterAccess);
        }
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;
}
