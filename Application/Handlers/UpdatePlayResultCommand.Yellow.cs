using System.Globalization;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private partial async ValueTask<uint> HandleYellow(
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
            logger.LogWarning("Game uploading a non existing Yellow user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        if (IsYellowTokkunShaped(playResultData))
        {
            return 1;
        }

        var saveData = await context.GetOrCreateYellowSaveDataAsync(request.Baid, cancellationToken);
        if (!CanAddYellow(saveData.TotalGetDonmedal, playResultData.GetDonmedal)
            || !CanAddYellow(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal))
        {
            logger.LogWarning("Rejecting invalid Yellow medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        var playTime = ParseYellowPlayDatetimeOrNow(playResultData.PlayDatetime);

        saveData.TotalGetDonmedal += playResultData.GetDonmedal;
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
            ApplyYellowCostume(saveData, playResultData.AryCurrentCostume);
        }

        ApplyYellowUnlockBits(saveData, playResultData);

        foreach (var stage in playResultData.AryStageInfoes)
        {
            ApplyYellowProfileStage(saveData, stage);
        }

        return await Ac15NormalPlayService.SaveAsync(
            request.Baid,
            playResultData,
            Ac15EraProfiles.Yellow,
            new YellowAc15NormalPlayAdapter(context),
            DefaultAc15EraHooks.Instance,
            cancellationToken);
    }

    private static bool IsYellowTokkunShaped(CommonPlayResultData playResultData)
        => playResultData.IsTokkunPlayResult
           || playResultData.PlayMode == (uint)PlayMode.Tokkun
           || playResultData.TokkunStageData is not null;

    private static void ApplyYellowCostume(UserSaveDataYellow saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        var limits = Ac15EraProfiles.Yellow.Limits;
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [costume.Costume1], limits.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [costume.Costume2], limits.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [costume.Costume3], limits.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [costume.Costume4], limits.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [costume.Costume5], limits.CostumeFlagBytes);
    }

    private static void ApplyYellowUnlockBits(UserSaveDataYellow saveData, CommonPlayResultData playResultData)
    {
        var limits = Ac15EraProfiles.Yellow.Limits;
        saveData.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(saveData.ReleaseSongFlg, playResultData.ReleaseSongNoes, limits.SongFlagBytes);
        saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, playResultData.GetToneNoes, limits.ToneFlagBytes);
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, limits.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, limits.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, limits.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, limits.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, limits.CostumeFlagBytes);
        saveData.TitleFlg = Ac15ProtocolBytes.SetBits(saveData.TitleFlg, playResultData.GetTitleNoes, limits.TitleFlagBytes);
    }

    private static void ApplyYellowProfileStage(UserSaveDataYellow saveData, CommonPlayResultData.StageData stage)
    {
        IncrementYellowGenreCounter(saveData, stage.MusicCateg);
        if (stage.IsPushed) saveData.SongPushedCnt = SafeYellowIncrement(saveData.SongPushedCnt);
        if (stage.IsFavorite) saveData.SongFavoriteCnt = SafeYellowIncrement(saveData.SongFavoriteCnt);
        if (stage.IsRecent) saveData.SongRecentCnt = SafeYellowIncrement(saveData.SongRecentCnt);
    }

    private static void IncrementYellowGenreCounter(UserSaveDataYellow saveData, uint musicCateg)
    {
        switch (musicCateg)
        {
            case 1: saveData.CategJpopCnt = SafeYellowIncrement(saveData.CategJpopCnt); break;
            case 2: saveData.CategAnimeCnt = SafeYellowIncrement(saveData.CategAnimeCnt); break;
            case 3: saveData.CategVocaloidCnt = SafeYellowIncrement(saveData.CategVocaloidCnt); break;
            case 4: saveData.CategDoyoCnt = SafeYellowIncrement(saveData.CategDoyoCnt); break;
            case 5: saveData.CategVarietyCnt = SafeYellowIncrement(saveData.CategVarietyCnt); break;
            case 6: saveData.CategClassicCnt = SafeYellowIncrement(saveData.CategClassicCnt); break;
            case 7: saveData.CategGameCnt = SafeYellowIncrement(saveData.CategGameCnt); break;
            case 8: saveData.CategNamcoCnt = SafeYellowIncrement(saveData.CategNamcoCnt); break;
        }
    }

    private static uint SafeYellowIncrement(uint current)
        => current == uint.MaxValue ? current : current + 1;

    private static bool CanAddYellow(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static DateTime ParseYellowPlayDatetimeOrNow(string playDatetime)
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
}
