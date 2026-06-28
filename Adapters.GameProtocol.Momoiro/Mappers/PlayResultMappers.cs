using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Mappers;

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

    [MapProperty(nameof(PlayResultRequest.GetDonpoint), nameof(Ac15ProfileMutationFacts.GetDonpoint), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.RewardPtn), Use = nameof(MapRewardPtn))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.RewardProgress), Use = nameof(MapRewardProgress))]
    [MapProperty(nameof(PlayResultRequest.ToneFlg), nameof(Ac15ProfileMutationFacts.GetToneNoes), Use = nameof(MapToneFlags))]
    [MapProperty(nameof(PlayResultRequest.CostumeFlg1), nameof(Ac15ProfileMutationFacts.GetCostumeNo1s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(PlayResultRequest.CostumeFlg2), nameof(Ac15ProfileMutationFacts.GetCostumeNo2s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(PlayResultRequest.CostumeFlg3), nameof(Ac15ProfileMutationFacts.GetCostumeNo3s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(PlayResultRequest.CostumeFlg4), nameof(Ac15ProfileMutationFacts.GetCostumeNo4s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(PlayResultRequest.CostumeFlg5), nameof(Ac15ProfileMutationFacts.GetCostumeNo5s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(PlayResultRequest.TitleFlg), nameof(Ac15ProfileMutationFacts.GetTitleNoes), Use = nameof(MapTitleFlags))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.IsDevil), Use = nameof(MapIsDevil))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.IsExplain), Use = nameof(MapIsExplain))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasAryCurrentCostume), Use = nameof(HasCurrentCostume))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.AryCurrentCostume), Use = nameof(MapCurrentCostume))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.GetDonmedal))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.GetKatsumedal))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.DifficultyTutorialFlg))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.ItemshopTutorialFlg))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.WaiwaiTutorialFlg))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedCourse))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.DifficultyPlayedCourse))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedStar))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.DifficultyPlayedStar))]
    private static partial Ac15ProfileMutationFacts MapProfile(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15NormalPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15NormalPlayResult MapNormalCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.DanResult), nameof(Ac15DaniPlayResult.DanResult), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapValue(nameof(Ac15DaniPlayResult.ComboCntTotal), 0u)]
    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15DaniPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15DaniPlayResult MapDani(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.StageData.Level), nameof(Ac15StageResult.Level), Use = nameof(@Ac15MapperNormalization.FromProtocolDifficulty))]
    [MapProperty(nameof(PlayResultRequest.StageData.HitCnt), nameof(Ac15StageResult.HitCnt), Use = nameof(@Ac15MapperNormalization.UIntOrZero))]
    [MapProperty(nameof(PlayResultRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.StageData.ToneFlg), nameof(Ac15StageResult.ToneFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(PlayResultRequest.StageData.PlayDan), nameof(Ac15StageResult.PlayDan), Use = nameof(@Ac15MapperNormalization.PositivePlayDan))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryChallengeIds), nameof(Ac15StageResult.ChallengeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryUserCompeIds), nameof(Ac15StageResult.UserCompeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryBngCompeIds), nameof(Ac15StageResult.BngCompeIds), Use = nameof(MapCompeList))]
    [MapPropertyFromSource(nameof(Ac15StageResult.SoulGauge), Use = nameof(MapSoulGauge))]
    [MapPropertyFromSource(nameof(Ac15StageResult.HitCount), Use = nameof(MapHitCount))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRate))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRank))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.SelectedFolderId))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.SupportLevel))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.StarLevel))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.IsWin))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.WaiwaiResult))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.WaiwaiGauge))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.BlueBattleStage))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.GreenGhostStage))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.AiSectionData))]
    [UserMapping(Default = true)]
    private static partial Ac15StageResult MapStage(PlayResultRequest.StageData stage);

    [UserMapping(Default = true)]
    private static partial Ac15CostumeFacts MapCostumeData(PlayResultRequest.CustumeData costume);

    [UserMapping(Default = true)]
    private static partial Ac15CompeIdFact MapCompe(PlayResultRequest.StageData.ResultcompeData data);

    private static Ac15NormalPlayResult? MapNormal(PlayResultRequest request)
        => request.AryStageInfoes.Count == 0 ? null : MapNormalCore(request);

    private static List<Ac15StageResult> MapStages(List<PlayResultRequest.StageData> stages)
        => stages.Select(MapStage).ToList();

    private static List<Ac15CompeIdFact> MapCompeList(List<PlayResultRequest.StageData.ResultcompeData> values)
        => values.Select(MapCompe).ToList();

    private static bool HasCurrentCostume(PlayResultRequest request) => request.AryCurrentCostume is not null;

    private static Ac15CostumeFacts MapCurrentCostume(PlayResultRequest request)
        => request.AryCurrentCostume is null ? Ac15CostumeFacts.Empty : MapCostumeData(request.AryCurrentCostume);

    private static uint? MapRewardPtn(PlayResultRequest request)
        => request.ShouldSerializeRewardPtn() ? request.RewardPtn : null;

    private static uint? MapRewardProgress(PlayResultRequest request)
        => request.ShouldSerializeRewardProgress() ? request.RewardProgress : null;

    private static List<uint> MapToneFlags(byte[]? values)
        => Ac15MapperNormalization.DecodeBitset(values, Ac15EraProfiles.Momoiro.Limits.ToneFlagBytes);

    private static List<uint> MapCostumeFlags(byte[]? values)
        => Ac15MapperNormalization.DecodeBitset(values, Ac15EraProfiles.Momoiro.Limits.CostumeFlagBytes);

    private static List<uint> MapTitleFlags(byte[]? values)
        => Ac15MapperNormalization.DecodeBitset(values, Ac15EraProfiles.Momoiro.Limits.TitleFlagBytes);

    private static bool? MapIsDevil(PlayResultRequest request)
        => request.ShouldSerializeIsDevil() ? request.IsDevil : null;

    private static bool? MapIsExplain(PlayResultRequest request)
        => request.ShouldSerializeIsExplain() ? request.IsExplain : null;

    private static uint? MapSoulGauge(PlayResultRequest.StageData stage)
        => stage.ShouldSerializeSoulGauge() ? stage.SoulGauge : null;

    private static uint? MapHitCount(PlayResultRequest.StageData stage)
        => stage.ShouldSerializeHitCnt() ? stage.HitCnt : null;

}
