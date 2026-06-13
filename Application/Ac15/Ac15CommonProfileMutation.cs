using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CommonProfileMutation
{
    public static bool TryApply<TSave>(
        TSave saveData,
        IAc15ShopSeasonState? shopSeasonState,
        Ac15ProfileMutationFacts profile,
        IReadOnlyList<Ac15StageResult> countedStages,
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
        if (!CanAdd(currentDonmedal, profile.GetDonmedal)
            || !CanAdd(saveData.TotalGetKatsumedal, profile.GetKatsumedal))
        {
            return false;
        }

        if (shopSeasonState is null)
        {
            saveData.TotalGetDonmedal += profile.GetDonmedal;
        }
        else
        {
            shopSeasonState.TotalGetDonmedal += profile.GetDonmedal;
            shopSeasonState.UpdatedAt = DateTime.UtcNow;
        }

        saveData.TotalGetKatsumedal += profile.GetKatsumedal;
        saveData.ItemshopTutorialFlg = profile.ItemshopTutorialFlg ?? saveData.ItemshopTutorialFlg;
        saveData.WaiwaiTutorialFlg = profile.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;

        ApplyShared(
            saveData,
            profile,
            countedStages,
            counterAccess,
            unlockAccess,
            limits,
            playTime);

        return true;
    }

    public static bool TryApplyDonPoints<TSave>(
        TSave saveData,
        Ac15ProfileMutationFacts profile,
        IReadOnlyList<Ac15StageResult> countedStages,
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
        if (!CanAdd(saveData.TotalGetDonpoint, profile.GetDonpoint))
        {
            return false;
        }

        saveData.TotalGetDonpoint += profile.GetDonpoint;
        saveData.RewardPtn = profile.RewardPtn ?? saveData.RewardPtn;
        saveData.RewardProgress = profile.RewardProgress ?? saveData.RewardProgress;
        saveData.DifficultyTutorialFlg = profile.DifficultyTutorialFlg ?? saveData.DifficultyTutorialFlg;

        ApplyShared(
            saveData,
            profile,
            countedStages,
            counterAccess,
            unlockAccess,
            limits,
            playTime);

        return true;
    }

    private static void ApplyShared<TSave>(
        TSave saveData,
        Ac15ProfileMutationFacts profile,
        IReadOnlyList<Ac15StageResult> countedStages,
        Ac15ProfileCounterAccess<TSave> counterAccess,
        Ac15UnlockFlagAccess<TSave> unlockAccess,
        Ac15ProtocolLimits limits,
        DateTime playTime)
        where TSave :
            IAc15PlayTutorialSaveData,
            IAc15PlayProfileSaveData,
            IAc15CustomizationSaveData
    {
        saveData.IsDevil = profile.IsDevil ?? saveData.IsDevil;
        saveData.IsExplain = profile.IsExplain ?? saveData.IsExplain;
        if (profile.HasDifficultyPlayedCourse)
        {
            saveData.DifficultyPlayedCourse = profile.DifficultyPlayedCourse;
        }

        if (profile.HasDifficultyPlayedStar)
        {
            saveData.DifficultyPlayedStar = profile.DifficultyPlayedStar;
        }

        saveData.LastPlayDatetime = playTime;
        saveData.PrevAreaCode = profile.AreaCode;

        if (profile.HasAryCurrentCostume && saveData.IsAutoCostumeOn)
        {
            Ac15CustomizationMutation.ApplyCurrentCostume(saveData, ToCommonCostume(profile.AryCurrentCostume), limits);
        }

        unlockAccess.ReleaseSongs?.Invoke(saveData, profile.ReleaseSongNoes.Where(id => id < (uint)limits.SongFlagBytes * 8));
        unlockAccess.Tones(saveData, profile.GetToneNoes.Where(id => id < (uint)limits.ToneFlagBytes * 8));
        unlockAccess.Titles(saveData, profile.GetTitleNoes.Where(id => id < (uint)limits.TitleFlagBytes * 8));
        unlockAccess.Costume1(saveData, profile.GetCostumeNo1s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume2(saveData, profile.GetCostumeNo2s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume3(saveData, profile.GetCostumeNo3s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume4(saveData, profile.GetCostumeNo4s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));
        unlockAccess.Costume5(saveData, profile.GetCostumeNo5s.Where(id => id < (uint)limits.CostumeFlagBytes * 8));

        foreach (var stage in countedStages)
        {
            Ac15ProfileCounterUpdater.ApplyStage(saveData, stage, counterAccess);
        }
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static CommonPlayResultData.CostumeData ToCommonCostume(Ac15CostumeFacts costume)
        => new()
        {
            Costume1 = costume.Costume1,
            Costume2 = costume.Costume2,
            Costume3 = costume.Costume3,
            Costume4 = costume.Costume4,
            Costume5 = costume.Costume5
        };
}
