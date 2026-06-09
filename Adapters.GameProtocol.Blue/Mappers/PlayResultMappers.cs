using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static CommonPlayResultData Map(PlayResultRequest request)
    {
        return new CommonPlayResultData
        {
            Baid = request.Baid,
            ChassisId = request.ChassisId,
            ShopId = request.ShopId,
            PlayDatetime = request.PlayDatetime,
            IsRight = request.IsRight,
            CardType = request.CardType,
            IsTwoPlayers = request.IsTwoPlayers,
            AryStageInfoes = request.AryStageInfoes.Select(MapStage).ToList(),
            ReleaseSongNoes = (request.ReleaseSongNoes ?? []).ToList(),
            GetToneNoes = (request.GetToneNoes ?? []).ToList(),
            GetCostumeNo1s = (request.GetCostumeNo1s ?? []).ToList(),
            GetCostumeNo2s = (request.GetCostumeNo2s ?? []).ToList(),
            GetCostumeNo3s = (request.GetCostumeNo3s ?? []).ToList(),
            GetCostumeNo4s = (request.GetCostumeNo4s ?? []).ToList(),
            GetCostumeNo5s = (request.GetCostumeNo5s ?? []).ToList(),
            GetTitleNoes = (request.GetTitleNoes ?? []).ToList(),
            GetDonmedal = request.GetDonmedal,
            GetKatsumedal = request.GetKatsumedal,
            BonusDailyFlg = request.BonusDailyFlg,
            BonusWeeklyFlg = request.BonusWeeklyFlg,
            BonusMonthlyFlg = request.BonusMonthlyFlg,
            ItemshopTutorialFlg = request.ItemshopTutorialFlg,
            IsDevil = request.IsDevil,
            IsExplain = request.IsExplain,
            AryPlayCostume = MapCostume(request.AryPlayCostume),
            AryCurrentCostume = MapCostume(request.AryCurrentCostume),
            HasAryCurrentCostume = request.AryCurrentCostume is not null,
            GenderType = request.GenderType,
            PlayerAge = request.PlayerAge,
            PlayMode = request.PlayMode,
            IsTokkunPlayResult = request.PlayMode == (uint)PlayMode.Tokkun,
            TokkunTutorialFlg = request.TokkunTutorialFlg,
            TokkunStageData = MapTokkunStageData(request.AryTokkunstageInfo),
            IsBattlePlayResult = request.AryReleaseBattledata is not null
                || request.AryStageInfoes.Any(stage => stage.AryBattlestagedata is not null),
            BattleReleaseData = MapBattleReleaseData(request.AryReleaseBattledata),
            AreaCode = request.AreaCode,
            Reserved = request.Reserved ?? [],
            LowerlimitAge = request.LowerlimitAge,
            UpperlimitAge = request.UpperlimitAge,
            AgeScore = request.AgeScore,
            EstimationCount = request.EstimationCount,
            DanResult = request.DanResult.GetValueOrDefault(),
            Accesstoken = request.Accesstoken,
            ContentInfo = request.ContentInfo ?? [],
            DifficultyPlayedCourse = request.DifficultyPlayedCourse.GetValueOrDefault(),
            DifficultyPlayedStar = request.DifficultyPlayedStar.GetValueOrDefault(),
            HasDifficultyPlayedCourse = request.DifficultyPlayedCourse is not null,
            HasDifficultyPlayedStar = request.DifficultyPlayedStar is not null,
            WaiwaiTutorialFlg = request.WaiwaiTutorialFlg
        };
    }

    public static PlayResultResponse Map(uint result)
    {
        return new PlayResultResponse { Result = result };
    }

    private static CommonPlayResultData.StageData MapStage(PlayResultRequest.StageData stage)
    {
        return new CommonPlayResultData.StageData
        {
            SongNo = stage.SongNo,
            Level = stage.Level,
            PlayResult = stage.PlayResult.GetValueOrDefault(),
            PlayScore = stage.PlayScore.GetValueOrDefault(),
            GoodCnt = stage.GoodCnt,
            OkCnt = stage.OkCnt,
            NgCnt = stage.NgCnt,
            PoundCnt = stage.PoundCnt,
            ComboCnt = stage.ComboCnt,
            HitCnt = stage.HitCnt.GetValueOrDefault(),
            OptionFlg = stage.OptionFlg ?? [],
            ToneFlg = stage.ToneFlg ?? [],
            AryChallengeIds = stage.AryChallengeIds.Select(MapCompe).ToList(),
            AryUserCompeIds = stage.AryUserCompeIds.Select(MapCompe).ToList(),
            AryBngCompeIds = stage.AryBngCompeIds.Select(MapCompe).ToList(),
            MusicCateg = stage.MusicCateg,
            IsPushed = stage.IsPushed,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            IsPapamama = stage.IsPapamama,
            PlayDan = stage.PlayDan is > 0 ? stage.PlayDan : null,
            SoulGauge = stage.SoulGauge,
            StageMode = stage.StageMode.GetValueOrDefault(),
            BattleStageData = MapBattleStageData(stage.AryBattlestagedata),
            SelectedFolderId = stage.SelectedFolderId,
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge
        };
    }

    private static CommonPlayResultData.BattleStageData? MapBattleStageData(
        PlayResultRequest.StageData.BattleStageData? data)
        => data is null
            ? null
            : new CommonPlayResultData.BattleStageData
            {
                SupportLv = data.SupportLv,
                BattleStageId = data.BattleStageId,
                NpcData = MapBattleNpcData(data.NpcData),
                KillCnt = data.KillCnt,
                BossLife = data.BossLife,
                TotalDamage = data.TotalDamage,
                CriticalCnt = data.CriticalCnt,
                SpecialMoveCnt = data.SpecialMoveCnt
            };

    private static CommonPlayResultData.BattleNpcData? MapBattleNpcData(
        PlayResultRequest.StageData.BattleStageData.BattleNpcData? data)
        => data is null
            ? null
            : new CommonPlayResultData.BattleNpcData
            {
                NpcId = data.NpcId,
                AcquiredExp = data.AcquiredExp ?? string.Empty,
                TotalExp = data.TotalExp ?? string.Empty,
                Dpn = data.Dpn,
                NpcCostumeId = data.NpcCostumeId,
                SpecialId1 = data.SpecialId1,
                SpecialId2 = data.SpecialId2,
                SpecialId3 = data.SpecialId3,
                BondsLv = data.BondsLv
            };

    private static CommonPlayResultData.BattleReleaseDataDto? MapBattleReleaseData(
        PlayResultRequest.ReleaseBattleData? data)
        => data is null
            ? null
            : new CommonPlayResultData.BattleReleaseDataDto
            {
                ReleaseInfoIds = (data.ReleaseInfoIds ?? []).ToList(),
                ReleaseBattleStageIds = (data.ReleaseBattleStageIds ?? []).ToList(),
                ReleaseNpcIds = (data.ReleaseNpcIds ?? []).ToList(),
                ReleaseNpcCostumeIds = (data.ReleaseNpcCostumeIds ?? []).ToList(),
                ReleaseNpcSpecialIds = (data.ReleaseNpcSpecialIds ?? []).ToList(),
                BattleTokenData = data.AryBattletokendatas.Select(MapBattleTokenData).ToList(),
                AssignNextStageId = data.AssignNextStageId
            };

    private static CommonPlayResultData.TokkunStageDataDto? MapTokkunStageData(
        PlayResultRequest.TokkunstageData? data)
        => data is null
            ? null
            : new CommonPlayResultData.TokkunStageDataDto
            {
                BanacoinDatetime = data.BanacoinDatetime ?? string.Empty,
                TokkunSongCnt = data.TokkunSongCnt,
                TookunSongnoes = (data.TookunSongnoes ?? []).ToList(),
                TokkunSpeedchangeCnt = data.TokkunSpeedchangeCnt,
                TokkunAutoplayCnt = data.TokkunAutoplayCnt,
                TokkunJumpCnt = data.TokkunJumpCnt
            };

    private static CommonPlayResultData.BattleTokenData MapBattleTokenData(
        PlayResultRequest.ReleaseBattleData.BattleTokenData data)
        => MapBattleTokenDataCore(data);

    private static partial CommonPlayResultData.BattleTokenData MapBattleTokenDataCore(
        PlayResultRequest.ReleaseBattleData.BattleTokenData data);

    private static partial CommonPlayResultData.ResultcompeData MapCompe(
        PlayResultRequest.StageData.ResultcompeData data);

    private static CommonPlayResultData.CostumeData MapCostume(PlayResultRequest.CostumeData? costume)
    {
        return costume is null
            ? new CommonPlayResultData.CostumeData()
            : MapCostumeData(costume);
    }

    private static partial CommonPlayResultData.CostumeData MapCostumeData(PlayResultRequest.CostumeData costume);
}
