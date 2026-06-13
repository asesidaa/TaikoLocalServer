using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    [MapPropertyFromSource(nameof(CommonPlayResultData.HasAryCurrentCostume), Use = nameof(HasAryCurrentCostume))]
    [MapPropertyFromSource(nameof(CommonPlayResultData.HasDifficultyPlayedCourse), Use = nameof(HasDifficultyPlayedCourse))]
    [MapPropertyFromSource(nameof(CommonPlayResultData.HasDifficultyPlayedStar), Use = nameof(HasDifficultyPlayedStar))]
    [MapProperty(nameof(PlayResultDataRequest.Reserved), nameof(CommonPlayResultData.Reserved), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultDataRequest.ContentInfo), nameof(CommonPlayResultData.ContentInfo), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultDataRequest.GhostReleaseData), nameof(CommonPlayResultData.GhostReleaseData), Use = nameof(MapGhostRelease))]
    [MapProperty(nameof(PlayResultDataRequest.GhostUpdatePerfdata), nameof(CommonPlayResultData.GhostUpdatePerfData), Use = nameof(MapGhostUpdatePerfData))]
    [MapProperty(nameof(PlayResultDataRequest.GhostUpdateRank), nameof(CommonPlayResultData.GhostUpdateRankData), Use = nameof(MapGhostUpdateRank))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.Title))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.TitleplateId))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.CollaborationId))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.DanId))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.SoulGaugeTotal))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.ComboCntTotal))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsNotRecordedDan))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.UraReleaseSongNoes))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.GetGenericInfoNoes))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.TournamentMode))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.DifficultyPlayedSort))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsRandomUsePlay))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.InputMedian))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.InputVariance))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.AryCollaboInfo))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsTokkunPlayResult))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.TokkunTutorialFlg))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.TokkunStageData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsBattlePlayResult))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.BattleReleaseData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.GetDonpoint))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.RewardPtn))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.RewardProgress))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.DifficultyTutorialFlg))]
    public static partial CommonPlayResultData Map(PlayResultDataRequest request);

    public static PlayResultResponse Map(uint result)
        => new() { Result = result };

    [MapProperty(nameof(PlayResultDataRequest.StageData.OptionFlg), nameof(CommonPlayResultData.StageData.OptionFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.ToneFlg), nameof(CommonPlayResultData.StageData.ToneFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.PlayDan), nameof(CommonPlayResultData.StageData.PlayDan), Use = nameof(MapPlayDan))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.GhostStagedata), nameof(CommonPlayResultData.StageData.GhostStageData), Use = nameof(MapGhostStage))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.ScoreRate))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.ScoreRank))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.AryChallengeIds))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.AryUserCompeIds))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.AryBngCompeIds))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsWin))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.ArySectionDatas))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.HitCount))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.BattleStageData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.NotesPosition))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsVoiceOn))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsSkipOn))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsSkipUse))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsRandomUseStage))]
    private static partial CommonPlayResultData.StageData MapStage(PlayResultDataRequest.StageData stage);

    [MapProperty(nameof(PlayResultDataRequest.CostumeData.Costume1), nameof(CommonPlayResultData.CostumeData.Costume1))]
    [MapProperty(nameof(PlayResultDataRequest.CostumeData.Costume2), nameof(CommonPlayResultData.CostumeData.Costume2))]
    [MapProperty(nameof(PlayResultDataRequest.CostumeData.Costume3), nameof(CommonPlayResultData.CostumeData.Costume3))]
    [MapProperty(nameof(PlayResultDataRequest.CostumeData.Costume4), nameof(CommonPlayResultData.CostumeData.Costume4))]
    [MapProperty(nameof(PlayResultDataRequest.CostumeData.Costume5), nameof(CommonPlayResultData.CostumeData.Costume5))]
    private static partial CommonPlayResultData.CostumeData MapCostumeData(
        PlayResultDataRequest.CostumeData costume);

    [MapProperty(nameof(PlayResultDataRequest.UpdateGhostInfoData.ReleaseInfoIds), nameof(CommonPlayResultData.UpdateGhostInfoData.ReleaseInfoId))]
    [MapProperty(nameof(PlayResultDataRequest.UpdateGhostInfoData.AryTokendatas), nameof(CommonPlayResultData.UpdateGhostInfoData.AryTokendata))]
    private static partial CommonPlayResultData.UpdateGhostInfoData MapGhostReleaseCore(
        PlayResultDataRequest.UpdateGhostInfoData ghost);

    private static partial CommonPlayResultData.UpdateGhostPerfData MapGhostUpdatePerfDataCore(
        PlayResultDataRequest.UpdateGhostPerfData ghost);

    [MapProperty(nameof(PlayResultDataRequest.UpdateGhostRankData.AryWinningsDatas), nameof(CommonPlayResultData.UpdateGhostRankData.AryWinningsData))]
    private static partial CommonPlayResultData.UpdateGhostRankData MapGhostUpdateRankCore(
        PlayResultDataRequest.UpdateGhostRankData rank);

    [MapProperty(nameof(PlayResultDataRequest.StageData.GhostStageData.ArySectionDatas), nameof(CommonPlayResultData.GhostStageData.ArySectionData))]
    private static partial CommonPlayResultData.GhostStageData MapGhostStageCore(
        PlayResultDataRequest.StageData.GhostStageData ghost);

    private static partial CommonPlayResultData.GhostTokenData MapGhostTokenData(
        PlayResultDataRequest.UpdateGhostInfoData.GhostTokenData token);

    private static partial CommonPlayResultData.GhostWinningsData MapGhostWinningsData(
        PlayResultDataRequest.UpdateGhostRankData.UpdateGhostWinningsData row);

    private static partial CommonPlayResultData.GhostStageSectionData MapGhostStageSectionData(
        PlayResultDataRequest.StageData.GhostStageData.GhostStageSectionData section);

    private static CommonPlayResultData.CostumeData MapCostume(PlayResultDataRequest.CostumeData costume)
        => costume is null ? new CommonPlayResultData.CostumeData() : MapCostumeData(costume);

    private static CommonPlayResultData.UpdateGhostInfoData? MapGhostRelease(
        PlayResultDataRequest.UpdateGhostInfoData ghost)
        => ghost is null ? null : MapGhostReleaseCore(ghost);

    private static CommonPlayResultData.UpdateGhostPerfData? MapGhostUpdatePerfData(
        PlayResultDataRequest.UpdateGhostPerfData ghost)
        => ghost is null ? null : MapGhostUpdatePerfDataCore(ghost);

    private static CommonPlayResultData.UpdateGhostRankData? MapGhostUpdateRank(
        PlayResultDataRequest.UpdateGhostRankData rank)
        => rank is null ? null : MapGhostUpdateRankCore(rank);

    private static CommonPlayResultData.GhostStageData? MapGhostStage(
        PlayResultDataRequest.StageData.GhostStageData ghost)
        => ghost is null ? null : MapGhostStageCore(ghost);

    [UserMapping(Default = true)]
    private static List<uint> MapUIntList(uint[] values)
        => values is null ? [] : values.ToList();

    [UserMapping(Default = true)]
    private static uint MapNullableUInt(uint? value)
        => value.GetValueOrDefault();

    private static byte[] MapBytes(byte[] values)
        => values is null ? [] : values;

    private static uint? MapPlayDan(uint? value)
        => value is > 0 ? value : null;

    private static bool HasAryCurrentCostume(PlayResultDataRequest request)
        => request.AryCurrentCostume is not null;

    private static bool HasDifficultyPlayedCourse(PlayResultDataRequest request)
        => request.DifficultyPlayedCourse is not null;

    private static bool HasDifficultyPlayedStar(PlayResultDataRequest request)
        => request.DifficultyPlayedStar is not null;
}
