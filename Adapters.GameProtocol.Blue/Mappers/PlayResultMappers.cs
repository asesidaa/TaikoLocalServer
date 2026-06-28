using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

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
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Tokkun), Use = nameof(MapTokkun))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.BlueBattle), Use = nameof(MapBlueBattle))]
    [MapValue(nameof(Ac15PlayResultEnvelope.GreenGhost), null)]
    public static partial Ac15PlayResultEnvelope Map(PlayResultRequest request);

    [MapPropertyFromSource(nameof(PlayResultResponse.Result))]
    public static partial PlayResultResponse Map(uint result);

    [MapProperty(nameof(PlayResultRequest.ChassisId), nameof(Ac15PlayResultMetadata.ChassisId), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.ShopId), nameof(Ac15PlayResultMetadata.ShopId), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.PlayDatetime), nameof(Ac15PlayResultMetadata.PlayDatetime), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.Reserved), nameof(Ac15PlayResultMetadata.Reserved), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.Accesstoken), nameof(Ac15PlayResultMetadata.Accesstoken), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.ContentInfo), nameof(Ac15PlayResultMetadata.ContentInfo), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    private static partial Ac15PlayResultMetadata MapMetadata(PlayResultRequest request);

    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasAryCurrentCostume), Use = nameof(HasCurrentCostume))]
    [MapProperty(nameof(PlayResultRequest.DifficultyPlayedCourse), nameof(Ac15ProfileMutationFacts.DifficultyPlayedCourse), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedCourse), Use = nameof(HasDifficultyPlayedCourse))]
    [MapProperty(nameof(PlayResultRequest.DifficultyPlayedStar), nameof(Ac15ProfileMutationFacts.DifficultyPlayedStar), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedStar), Use = nameof(HasDifficultyPlayedStar))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.GetDonpoint))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.RewardPtn))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.RewardProgress))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.DifficultyTutorialFlg))]
    private static partial Ac15ProfileMutationFacts MapProfile(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15NormalPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15NormalPlayResult MapNormalCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.DanResult), nameof(Ac15DaniPlayResult.DanResult), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapValue(nameof(Ac15DaniPlayResult.ComboCntTotal), 0u)]
    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15DaniPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15DaniPlayResult MapDani(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.TokkunTutorialFlg), nameof(Ac15TokkunPlayResult.TutorialFlg))]
    [MapProperty(nameof(PlayResultRequest.AryTokkunstageInfo), nameof(Ac15TokkunPlayResult.StageData))]
    private static partial Ac15TokkunPlayResult MapTokkunCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.AryReleaseBattledata), nameof(Ac15BlueBattlePlayResult.ReleaseData))]
    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15BlueBattlePlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15BlueBattlePlayResult MapBlueBattleCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.StageData.PlayResult), nameof(Ac15StageResult.PlayResult), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultRequest.StageData.PlayScore), nameof(Ac15StageResult.PlayScore), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultRequest.StageData.Level), nameof(Ac15StageResult.Level), Use = nameof(@Ac15MapperNormalization.FromProtocolDifficulty))]
    [MapProperty(nameof(PlayResultRequest.StageData.HitCnt), nameof(Ac15StageResult.HitCnt), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.StageData.ToneFlg), nameof(Ac15StageResult.ToneFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.StageData.StageMode), nameof(Ac15StageResult.StageMode), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultRequest.StageData.PlayDan), nameof(Ac15StageResult.PlayDan), Use = nameof(@Ac15MapperNormalization.PositiveNullablePlayDan))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryBattlestagedata), nameof(Ac15StageResult.BlueBattleStage))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryChallengeIds), nameof(Ac15StageResult.ChallengeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryUserCompeIds), nameof(Ac15StageResult.UserCompeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryBngCompeIds), nameof(Ac15StageResult.BngCompeIds), Use = nameof(MapCompeList))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRate))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRank))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.SupportLevel))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.StarLevel))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.IsWin))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.HitCount))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.GreenGhostStage))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.AiSectionData))]
    [UserMapping(Default = true)]
    private static partial Ac15StageResult MapStage(PlayResultRequest.StageData stage);

    [UserMapping(Default = true)]
    private static partial Ac15CostumeFacts MapCostumeData(PlayResultRequest.CostumeData costume);

    [MapProperty(nameof(PlayResultRequest.TokkunstageData.BanacoinDatetime), nameof(Ac15TokkunStageData.BanacoinDatetime), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [UserMapping(Default = true)]
    private static partial Ac15TokkunStageData? MapTokkunStageData(PlayResultRequest.TokkunstageData? data);

    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleStageData? MapBattleStageData(PlayResultRequest.StageData.BattleStageData? data);

    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.BattleNpcData.AcquiredExp), nameof(Ac15BlueBattleNpcData.AcquiredExp), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.BattleNpcData.TotalExp), nameof(Ac15BlueBattleNpcData.TotalExp), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleNpcData? MapBattleNpcData(PlayResultRequest.StageData.BattleStageData.BattleNpcData? data);

    [MapProperty(nameof(PlayResultRequest.ReleaseBattleData.AryBattletokendatas), nameof(Ac15BlueBattleReleaseData.BattleTokenData), Use = nameof(MapBattleTokenDataList))]
    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleReleaseData? MapBattleReleaseData(PlayResultRequest.ReleaseBattleData? data);

    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleTokenData MapBattleTokenData(PlayResultRequest.ReleaseBattleData.BattleTokenData data);

    [UserMapping(Default = true)]
    private static partial Ac15CompeIdFact MapCompe(PlayResultRequest.StageData.ResultcompeData data);

    private static Ac15NormalPlayResult? MapNormal(PlayResultRequest request)
        => request.AryStageInfoes.Count == 0 ? null : MapNormalCore(request);

    private static Ac15TokkunPlayResult? MapTokkun(PlayResultRequest request)
        => request.TokkunTutorialFlg is null && request.AryTokkunstageInfo is null
            ? null
            : MapTokkunCore(request);

    private static Ac15BlueBattlePlayResult? MapBlueBattle(PlayResultRequest request)
        => request.AryReleaseBattledata is null && request.AryStageInfoes.All(stage => stage.AryBattlestagedata is null)
            ? null
            : MapBlueBattleCore(request);

    private static List<Ac15StageResult> MapStages(List<PlayResultRequest.StageData> stages)
        => stages.Select(MapStage).ToList();

    private static List<Ac15CompeIdFact> MapCompeList(List<PlayResultRequest.StageData.ResultcompeData> values)
        => values.Select(MapCompe).ToList();

    private static List<Ac15BlueBattleTokenData> MapBattleTokenDataList(
        List<PlayResultRequest.ReleaseBattleData.BattleTokenData> values)
        => values.Select(MapBattleTokenData).ToList();

    private static bool HasCurrentCostume(PlayResultRequest request) => request.AryCurrentCostume is not null;

    private static bool HasDifficultyPlayedCourse(PlayResultRequest request) => request.DifficultyPlayedCourse is not null;

    private static bool HasDifficultyPlayedStar(PlayResultRequest request) => request.DifficultyPlayedStar is not null;

}
