using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper(
    AllowNullPropertyAssignment = false,
    ThrowOnMappingNullMismatch = false,
    ThrowOnPropertyMappingNullMismatch = false)]
public static partial class PlayResultMappers
{
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Metadata), Use = nameof(MapMetadata))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Profile), Use = nameof(MapProfile))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Normal), Use = nameof(MapNormal))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Dani), Use = nameof(MapDani))]
    [MapValue(nameof(Ac15PlayResultEnvelope.Tokkun), null)]
    [MapValue(nameof(Ac15PlayResultEnvelope.BlueBattle), null)]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.GreenGhost), Use = nameof(MapGreenGhost))]
    public static partial Ac15PlayResultEnvelope Map(PlayResultDataRequest request);

    [MapPropertyFromSource(nameof(PlayResultResponse.Result))]
    public static partial PlayResultResponse Map(uint result);

    [MapProperty(nameof(PlayResultDataRequest.ChassisId), nameof(Ac15PlayResultMetadata.ChassisId), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultDataRequest.ShopId), nameof(Ac15PlayResultMetadata.ShopId), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultDataRequest.PlayDatetime), nameof(Ac15PlayResultMetadata.PlayDatetime), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultDataRequest.Reserved), nameof(Ac15PlayResultMetadata.Reserved), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultDataRequest.Accesstoken), nameof(Ac15PlayResultMetadata.Accesstoken), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultDataRequest.ContentInfo), nameof(Ac15PlayResultMetadata.ContentInfo), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    private static partial Ac15PlayResultMetadata MapMetadata(PlayResultDataRequest request);

    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasAryCurrentCostume), Use = nameof(HasCurrentCostume))]
    [MapProperty(nameof(PlayResultDataRequest.DifficultyPlayedCourse), nameof(Ac15ProfileMutationFacts.DifficultyPlayedCourse), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedCourse), Use = nameof(HasDifficultyPlayedCourse))]
    [MapProperty(nameof(PlayResultDataRequest.DifficultyPlayedStar), nameof(Ac15ProfileMutationFacts.DifficultyPlayedStar), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedStar), Use = nameof(HasDifficultyPlayedStar))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.GetDonpoint))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.RewardPtn))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.RewardProgress))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.DifficultyTutorialFlg))]
    private static partial Ac15ProfileMutationFacts MapProfile(PlayResultDataRequest request);

    [MapProperty(nameof(PlayResultDataRequest.AryStageInfoes), nameof(Ac15NormalPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15NormalPlayResult MapNormalCore(PlayResultDataRequest request);

    [MapProperty(nameof(PlayResultDataRequest.DanResult), nameof(Ac15DaniPlayResult.DanResult), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapValue(nameof(Ac15DaniPlayResult.ComboCntTotal), 0u)]
    [MapProperty(nameof(PlayResultDataRequest.AryStageInfoes), nameof(Ac15DaniPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15DaniPlayResult MapDani(PlayResultDataRequest request);

    [MapProperty(nameof(PlayResultDataRequest.GhostReleaseData), nameof(Ac15GreenGhostPlayResult.ReleaseData))]
    [MapProperty(nameof(PlayResultDataRequest.GhostUpdatePerfdata), nameof(Ac15GreenGhostPlayResult.PerfData))]
    [MapProperty(nameof(PlayResultDataRequest.GhostUpdateRank), nameof(Ac15GreenGhostPlayResult.RankData))]
    private static partial Ac15GreenGhostPlayResult MapGreenGhostCore(PlayResultDataRequest request);

    [MapProperty(nameof(PlayResultDataRequest.StageData.PlayResult), nameof(Ac15StageResult.PlayResult), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.PlayScore), nameof(Ac15StageResult.PlayScore), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.Level), nameof(Ac15StageResult.Level), Use = nameof(@Ac15MapperNormalization.FromProtocolDifficulty))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.HitCnt), nameof(Ac15StageResult.HitCnt), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.ToneFlg), nameof(Ac15StageResult.ToneFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.StageMode), nameof(Ac15StageResult.StageMode), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.PlayDan), nameof(Ac15StageResult.PlayDan), Use = nameof(@Ac15MapperNormalization.PositiveNullablePlayDan))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.GhostStagedata), nameof(Ac15StageResult.GreenGhostStage))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.AryChallengeIds), nameof(Ac15StageResult.ChallengeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.AryUserCompeIds), nameof(Ac15StageResult.UserCompeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultDataRequest.StageData.AryBngCompeIds), nameof(Ac15StageResult.BngCompeIds), Use = nameof(MapCompeList))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRate))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRank))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.IsWin))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.HitCount))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.BlueBattleStage))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.AiSectionData))]
    [UserMapping(Default = true)]
    private static partial Ac15StageResult MapStage(PlayResultDataRequest.StageData stage);

    [UserMapping(Default = true)]
    private static partial Ac15CostumeFacts MapCostumeData(PlayResultDataRequest.CostumeData costume);

    [MapProperty(nameof(PlayResultDataRequest.StageData.GhostStageData.ArySectionDatas), nameof(Ac15GreenGhostStageData.ArySectionData), Use = nameof(MapGhostStageSections))]
    [UserMapping(Default = true)]
    private static partial Ac15GreenGhostStageData? MapGhostStageData(PlayResultDataRequest.StageData.GhostStageData? data);

    [UserMapping(Default = true)]
    private static partial Ac15GreenGhostStageSectionData MapGhostStageSectionData(
        PlayResultDataRequest.StageData.GhostStageData.GhostStageSectionData data);

    [MapProperty(nameof(PlayResultDataRequest.UpdateGhostInfoData.ReleaseInfoIds), nameof(Ac15GreenGhostReleaseData.ReleaseInfoId), Use = nameof(@Ac15MapperNormalization.UIntList))]
    [MapProperty(nameof(PlayResultDataRequest.UpdateGhostInfoData.AryTokendatas), nameof(Ac15GreenGhostReleaseData.AryTokendata), Use = nameof(MapGhostTokenDataList))]
    [UserMapping(Default = true)]
    private static partial Ac15GreenGhostReleaseData? MapGhostReleaseData(PlayResultDataRequest.UpdateGhostInfoData? data);

    [UserMapping(Default = true)]
    private static partial Ac15GreenGhostTokenData MapGhostTokenData(PlayResultDataRequest.UpdateGhostInfoData.GhostTokenData data);

    [UserMapping(Default = true)]
    private static partial Ac15GreenGhostPerfData? MapGhostPerfData(PlayResultDataRequest.UpdateGhostPerfData? data);

    [MapProperty(nameof(PlayResultDataRequest.UpdateGhostRankData.AryWinningsDatas), nameof(Ac15GreenGhostRankData.AryWinningsData), Use = nameof(MapGhostWinningsDataList))]
    [UserMapping(Default = true)]
    private static partial Ac15GreenGhostRankData? MapGhostRankData(PlayResultDataRequest.UpdateGhostRankData? data);

    [UserMapping(Default = true)]
    private static partial Ac15GreenGhostWinningsData MapGhostWinningsData(
        PlayResultDataRequest.UpdateGhostRankData.UpdateGhostWinningsData data);

    [UserMapping(Default = true)]
    private static partial Ac15CompeIdFact MapCompe(PlayResultDataRequest.StageData.ResultcompeData data);

    private static Ac15NormalPlayResult? MapNormal(PlayResultDataRequest request)
        => request.AryStageInfoes.Count == 0 ? null : MapNormalCore(request);

    private static Ac15GreenGhostPlayResult? MapGreenGhost(PlayResultDataRequest request)
        => request.GhostReleaseData is null && request.GhostUpdatePerfdata is null && request.GhostUpdateRank is null
            ? null
            : MapGreenGhostCore(request);

    private static List<Ac15StageResult> MapStages(List<PlayResultDataRequest.StageData> stages)
        => stages.Select(MapStage).ToList();

    private static List<Ac15CompeIdFact> MapCompeList(List<PlayResultDataRequest.StageData.ResultcompeData> values)
        => values.Select(MapCompe).ToList();

    private static List<Ac15GreenGhostStageSectionData> MapGhostStageSections(
        List<PlayResultDataRequest.StageData.GhostStageData.GhostStageSectionData> values)
        => values.Select(MapGhostStageSectionData).ToList();

    private static List<Ac15GreenGhostTokenData> MapGhostTokenDataList(
        List<PlayResultDataRequest.UpdateGhostInfoData.GhostTokenData> values)
        => values.Select(MapGhostTokenData).ToList();

    private static List<Ac15GreenGhostWinningsData> MapGhostWinningsDataList(
        List<PlayResultDataRequest.UpdateGhostRankData.UpdateGhostWinningsData> values)
        => values.Select(MapGhostWinningsData).ToList();

    private static bool HasCurrentCostume(PlayResultDataRequest request) => request.AryCurrentCostume is not null;

    private static bool HasDifficultyPlayedCourse(PlayResultDataRequest request) => request.DifficultyPlayedCourse is not null;

    private static bool HasDifficultyPlayedStar(PlayResultDataRequest request) => request.DifficultyPlayedStar is not null;

}
