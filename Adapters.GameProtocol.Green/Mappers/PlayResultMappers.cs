using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static CommonPlayResultData Map(PlayResultDataRequest request)
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
            WaiwaiTutorialFlg = request.WaiwaiTutorialFlg,
            GhostReleaseData = MapGhostRelease(request.GhostReleaseData),
            GhostUpdatePerfData = request.GhostUpdatePerfdata is null ? null : new CommonPlayResultData.UpdateGhostPerfData
            {
                InputMedian = request.GhostUpdatePerfdata.InputMedian,
                InputVariance = request.GhostUpdatePerfdata.InputVariance
            },
            GhostUpdateRankData = MapGhostRank(request.GhostUpdateRank)
        };
    }

    public static PlayResultResponse Map(uint result)
    {
        return new PlayResultResponse { Result = result };
    }

    private static CommonPlayResultData.StageData MapStage(PlayResultDataRequest.StageData stage)
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
            SupportLevel = stage.SupportLevel,
            MusicCateg = stage.MusicCateg,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            SelectedFolderId = stage.SelectedFolderId,
            StarLevel = stage.StarLevel,
            SoulGauge = stage.SoulGauge,
            PlayDan = stage.PlayDan is > 0 ? stage.PlayDan : null,
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge,
            GhostStageData = MapGhostStage(stage.GhostStagedata),
            StageMode = stage.StageMode.GetValueOrDefault(),
            IsPapamama = stage.IsPapamama,
            IsPushed = stage.IsPushed
        };
    }

    private static CommonPlayResultData.CostumeData MapCostume(PlayResultDataRequest.CostumeData? costume)
    {
        return costume is null
            ? new CommonPlayResultData.CostumeData()
            : MapCostumeData(costume);
    }

    private static CommonPlayResultData.UpdateGhostInfoData? MapGhostRelease(
        PlayResultDataRequest.UpdateGhostInfoData? ghost)
    {
        return ghost is null
            ? null
            : new CommonPlayResultData.UpdateGhostInfoData
            {
                ReleaseInfoId = (ghost.ReleaseInfoIds ?? []).ToList(),
                AryTokendata = ghost.AryTokendatas.Select(MapGhostTokenData).ToList()
            };
    }

    private static CommonPlayResultData.UpdateGhostRankData? MapGhostRank(
        PlayResultDataRequest.UpdateGhostRankData? rank)
    {
        return rank is null
            ? null
            : new CommonPlayResultData.UpdateGhostRankData
            {
                RankId = rank.RankId,
                WinPoint = rank.WinPoint,
                CertifiedLevelId = rank.CertifiedLevelId,
                AryWinningsData = rank.AryWinningsDatas.Select(MapGhostWinningsData).ToList()
            };
    }

    private static CommonPlayResultData.GhostStageData? MapGhostStage(
        PlayResultDataRequest.StageData.GhostStageData? ghost)
    {
        return ghost is null
            ? null
            : new CommonPlayResultData.GhostStageData
            {
                IsWin = ghost.IsWin,
                SdCertifiedLevelId = ghost.SdCertifiedLevelId,
                ArySectionData = ghost.ArySectionDatas.Select(MapGhostStageSectionData).ToList()
            };
    }

    private static partial CommonPlayResultData.CostumeData MapCostumeData(
        PlayResultDataRequest.CostumeData costume);

    private static partial CommonPlayResultData.GhostTokenData MapGhostTokenData(
        PlayResultDataRequest.UpdateGhostInfoData.GhostTokenData token);

    private static partial CommonPlayResultData.GhostWinningsData MapGhostWinningsData(
        PlayResultDataRequest.UpdateGhostRankData.UpdateGhostWinningsData row);

    private static partial CommonPlayResultData.GhostStageSectionData MapGhostStageSectionData(
        PlayResultDataRequest.StageData.GhostStageData.GhostStageSectionData section);
}
