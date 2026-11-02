using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;
using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper(
    AllowNullPropertyAssignment = false,
    ThrowOnMappingNullMismatch = false,
    ThrowOnPropertyMappingNullMismatch = false)]
public static partial class FinalPlayResultMappers
{
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Metadata), Use = nameof(MapMetadata))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Profile), Use = nameof(MapProfile))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Normal), Use = nameof(MapNormal))]
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.Dani), Use = nameof(MapDani))]
    [MapValue(nameof(Ac15PlayResultEnvelope.Tokkun), null)]
    [MapValue(nameof(Ac15PlayResultEnvelope.BlueBattle), null)]
    [MapValue(nameof(Ac15PlayResultEnvelope.GreenGhost), null)]
    public static partial Ac15PlayResultEnvelope Map(FinalWire.PlayResultRequest request);

    [MapPropertyFromSource(nameof(FinalWire.PlayResultResponse.Result))]
    public static partial FinalWire.PlayResultResponse Map(uint result);

    [MapProperty(nameof(FinalWire.PlayResultRequest.ChassisId), nameof(Ac15PlayResultMetadata.ChassisId), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.ShopId), nameof(Ac15PlayResultMetadata.ShopId), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.PlayDatetime), nameof(Ac15PlayResultMetadata.PlayDatetime), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.Reserved), nameof(Ac15PlayResultMetadata.Reserved), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.Accesstoken), nameof(Ac15PlayResultMetadata.Accesstoken), Use = nameof(@Ac15MapperNormalization.StringOrEmpty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.ContentInfo), nameof(Ac15PlayResultMetadata.ContentInfo), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    private static partial Ac15PlayResultMetadata MapMetadata(FinalWire.PlayResultRequest request);

    [MapProperty(nameof(FinalWire.PlayResultRequest.ToneFlg), nameof(Ac15ProfileMutationFacts.GetToneNoes), Use = nameof(MapToneFlags))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.CostumeFlg1), nameof(Ac15ProfileMutationFacts.GetCostumeNo1s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.CostumeFlg2), nameof(Ac15ProfileMutationFacts.GetCostumeNo2s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.CostumeFlg3), nameof(Ac15ProfileMutationFacts.GetCostumeNo3s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.CostumeFlg4), nameof(Ac15ProfileMutationFacts.GetCostumeNo4s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.CostumeFlg5), nameof(Ac15ProfileMutationFacts.GetCostumeNo5s), Use = nameof(MapCostumeFlags))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.TitleFlg), nameof(Ac15ProfileMutationFacts.GetTitleNoes), Use = nameof(MapTitleFlags))]
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
    private static partial Ac15ProfileMutationFacts MapProfile(FinalWire.PlayResultRequest request);

    [MapProperty(nameof(FinalWire.PlayResultRequest.AryStageInfoes), nameof(Ac15NormalPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15NormalPlayResult MapNormalCore(FinalWire.PlayResultRequest request);

    [MapValue(nameof(Ac15DaniPlayResult.ComboCntTotal), 0u)]
    [MapProperty(nameof(FinalWire.PlayResultRequest.AryStageInfoes), nameof(Ac15DaniPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15DaniPlayResult MapDani(FinalWire.PlayResultRequest request);

    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.Level), nameof(Ac15StageResult.Level), Use = nameof(@Ac15MapperNormalization.FromProtocolDifficulty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.ToneFlg), nameof(Ac15StageResult.ToneFlg), Use = nameof(@Ac15MapperNormalization.BytesOrEmpty))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.PlayDan), nameof(Ac15StageResult.PlayDan), Use = nameof(@Ac15MapperNormalization.PositivePlayDan))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.AryChallengeIds), nameof(Ac15StageResult.ChallengeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.AryUserCompeIds), nameof(Ac15StageResult.UserCompeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.AryBngCompeIds), nameof(Ac15StageResult.BngCompeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(FinalWire.PlayResultRequest.StageData.HitCnt), nameof(Ac15StageResult.HitCount))]
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
    private static partial Ac15StageResult MapStage(FinalWire.PlayResultRequest.StageData stage);

    [UserMapping(Default = true)]
    private static partial Ac15CostumeFacts MapCostumeData(FinalWire.PlayResultRequest.CostumeData costume);

    [UserMapping(Default = true)]
    private static partial Ac15CompeIdFact MapCompe(FinalWire.PlayResultRequest.StageData.ResultcompeData data);

    private static Ac15NormalPlayResult? MapNormal(FinalWire.PlayResultRequest request)
        => request.AryStageInfoes.Count == 0 ? null : MapNormalCore(request);

    private static List<Ac15StageResult> MapStages(List<FinalWire.PlayResultRequest.StageData> stages)
        => stages.Select(MapStage).ToList();

    private static List<Ac15CompeIdFact> MapCompeList(List<FinalWire.PlayResultRequest.StageData.ResultcompeData> values)
        => values.Select(MapCompe).ToList();

    private static bool HasCurrentCostume(FinalWire.PlayResultRequest request) => request.AryCurrentCostume is not null;

    private static Ac15CostumeFacts MapCurrentCostume(FinalWire.PlayResultRequest request)
        => request.AryCurrentCostume is null ? Ac15CostumeFacts.Empty : MapCostumeData(request.AryCurrentCostume);

    private static List<uint> MapToneFlags(byte[]? values)
        => Ac15MapperNormalization.DecodeBitset(values, Ac15EraProfiles.Kimidori.Limits.ToneFlagBytes);

    private static List<uint> MapCostumeFlags(byte[]? values)
        => Ac15MapperNormalization.DecodeBitset(values, Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes);

    private static List<uint> MapTitleFlags(byte[]? values)
        => Ac15MapperNormalization.DecodeBitset(values, Ac15EraProfiles.Kimidori.Limits.TitleFlagBytes);

}
