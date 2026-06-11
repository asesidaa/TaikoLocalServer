using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CommonProfileMutationTests
{
    [Fact]
    public void TryApply_AddsDonMedalsToActiveShopSeasonAndKatsuToSave()
    {
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        var season = new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 4,
            TotalGetDonmedal = 100,
            TotalUseDonmedal = 20,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var applied = Ac15CommonProfileMutation.TryApply(
            save,
            season,
            PlayResult(getDonmedal: 25, getKatsumedal: 5),
            countedStages: [Stage(101, isFavorite: true)],
            Ac15ProfileCounterUpdater.Blue,
            Ac15UnlockFlagAccess.Blue,
            Ac15EraProfiles.Blue.Limits,
            new DateTime(2026, 5, 14, 3, 24, 42),
            ApplyBlueCostume);

        Assert.True(applied);
        Assert.Equal(125u, season.TotalGetDonmedal);
        Assert.Equal(0u, save.TotalGetDonmedal);
        Assert.Equal(5u, save.TotalGetKatsumedal);
        Assert.Equal(1u, save.SongFavoriteCnt);
        Assert.Equal(new DateTime(2026, 5, 14, 3, 24, 42), save.LastPlayDatetime);
    }

    [Fact]
    public void TryApply_DetectsDonMedalOverflowBeforeMutation()
    {
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.TotalGetDonmedal = uint.MaxValue;

        var applied = Ac15CommonProfileMutation.TryApply(
            save,
            shopSeasonState: null,
            PlayResult(getDonmedal: 1, getKatsumedal: 0),
            countedStages: [Stage(101)],
            Ac15ProfileCounterUpdater.Blue,
            Ac15UnlockFlagAccess.Blue,
            Ac15EraProfiles.Blue.Limits,
            DateTime.UnixEpoch,
            ApplyBlueCostume);

        Assert.False(applied);
        Assert.Equal(uint.MaxValue, save.TotalGetDonmedal);
        Assert.Equal(0u, save.SongRecentCnt);
    }

    [Fact]
    public void GreenUnlockPolicyLeavesSongReleaseFlagsAbsent()
    {
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);

        var applied = Ac15CommonProfileMutation.TryApply(
            save,
            shopSeasonState: null,
            new CommonPlayResultData
            {
                ReleaseSongNoes = [101],
                GetToneNoes = [5],
                AryStageInfoes = [Stage(101)]
            },
            countedStages: [Stage(101)],
            Ac15ProfileCounterUpdater.Green,
            Ac15UnlockFlagAccess.Green,
            Ac15EraProfiles.Green.Limits,
            DateTime.UnixEpoch,
            ApplyGreenCostume);

        Assert.True(applied);
        Assert.True(BitIsSet(save.ToneFlg, 5));
    }

    private static CommonPlayResultData PlayResult(uint getDonmedal, uint getKatsumedal)
        => new()
        {
            GetDonmedal = getDonmedal,
            GetKatsumedal = getKatsumedal,
            AreaCode = 10,
            IsDevil = true,
            IsExplain = true,
            WaiwaiTutorialFlg = 1,
            HasDifficultyPlayedCourse = true,
            DifficultyPlayedCourse = 3,
            HasDifficultyPlayedStar = true,
            DifficultyPlayedStar = 4,
            AryStageInfoes = [Stage(101)]
        };

    private static CommonPlayResultData.StageData Stage(uint songNo, bool isFavorite = false)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            StageMode = 0,
            MusicCateg = 2,
            IsFavorite = isFavorite,
            IsRecent = true
        };

    private static void ApplyBlueCostume(UserSaveDataBlue save, CommonPlayResultData.CostumeData costume)
    {
        save.Costume1 = costume.Costume1;
        save.Costume2 = costume.Costume2;
        save.Costume3 = costume.Costume3;
        save.Costume4 = costume.Costume4;
        save.Costume5 = costume.Costume5;
    }

    private static void ApplyGreenCostume(UserSaveDataGreen save, CommonPlayResultData.CostumeData costume)
    {
        save.Costume1 = costume.Costume1;
        save.Costume2 = costume.Costume2;
        save.Costume3 = costume.Costume3;
        save.Costume4 = costume.Costume4;
        save.Costume5 = costume.Costume5;
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
