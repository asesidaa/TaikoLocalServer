using Riok.Mapperly.Abstractions;

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
    [MapPropertyFromSource(nameof(Ac15PlayResultEnvelope.ChallengeCompe), Use = nameof(MapChallengeCompe))]
    public static partial Ac15PlayResultEnvelope Map(PlayResultRequest request);

    [MapPropertyFromSource(nameof(PlayResultResponse.Result))]
    public static partial PlayResultResponse Map(uint result);

    [MapProperty(nameof(PlayResultRequest.ChassisId), nameof(Ac15PlayResultMetadata.ChassisId), Use = nameof(MapString))]
    [MapProperty(nameof(PlayResultRequest.ShopId), nameof(Ac15PlayResultMetadata.ShopId), Use = nameof(MapString))]
    [MapProperty(nameof(PlayResultRequest.PlayDatetime), nameof(Ac15PlayResultMetadata.PlayDatetime), Use = nameof(MapString))]
    [MapProperty(nameof(PlayResultRequest.Reserved), nameof(Ac15PlayResultMetadata.Reserved), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.Accesstoken), nameof(Ac15PlayResultMetadata.Accesstoken), Use = nameof(MapString))]
    [MapProperty(nameof(PlayResultRequest.ContentInfo), nameof(Ac15PlayResultMetadata.ContentInfo), Use = nameof(MapBytes))]
    private static partial Ac15PlayResultMetadata MapMetadata(PlayResultRequest request);

    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasAryCurrentCostume), Use = nameof(HasCurrentCostume))]
    [MapProperty(nameof(PlayResultRequest.DifficultyPlayedCourse), nameof(Ac15ProfileMutationFacts.DifficultyPlayedCourse), Use = nameof(MapNullableUInt))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedCourse), Use = nameof(HasDifficultyPlayedCourse))]
    [MapProperty(nameof(PlayResultRequest.DifficultyPlayedStar), nameof(Ac15ProfileMutationFacts.DifficultyPlayedStar), Use = nameof(MapNullableUInt))]
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasDifficultyPlayedStar), Use = nameof(HasDifficultyPlayedStar))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.GetDonpoint))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.RewardPtn))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.RewardProgress))]
    [MapperIgnoreTarget(nameof(Ac15ProfileMutationFacts.DifficultyTutorialFlg))]
    private static partial Ac15ProfileMutationFacts MapProfile(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15NormalPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15NormalPlayResult MapNormalCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.DanResult), nameof(Ac15DaniPlayResult.DanResult), Use = nameof(MapNullableUInt))]
    [MapValue(nameof(Ac15DaniPlayResult.ComboCntTotal), 0u)]
    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15DaniPlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15DaniPlayResult MapDani(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.TokkunTutorialFlg), nameof(Ac15TokkunPlayResult.TutorialFlg))]
    [MapProperty(nameof(PlayResultRequest.AryTokkunstageInfo), nameof(Ac15TokkunPlayResult.StageData))]
    private static partial Ac15TokkunPlayResult MapTokkunCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.AryReleaseBattledata), nameof(Ac15BlueBattlePlayResult.ReleaseData))]
    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15BlueBattlePlayResult.Stages), Use = nameof(MapStages))]
    private static partial Ac15BlueBattlePlayResult MapBlueBattleCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.AryStageInfoes), nameof(Ac15RedChallengeCompeFacts.Stages), Use = nameof(MapChallengeCompeStages))]
    private static partial Ac15RedChallengeCompeFacts MapChallengeCompeCore(PlayResultRequest request);

    [MapProperty(nameof(PlayResultRequest.StageData.PlayResult), nameof(Ac15StageResult.PlayResult), Use = nameof(MapNullableUInt))]
    [MapProperty(nameof(PlayResultRequest.StageData.PlayScore), nameof(Ac15StageResult.PlayScore), Use = nameof(MapNullableUInt))]
    [MapProperty(nameof(PlayResultRequest.StageData.HitCnt), nameof(Ac15StageResult.HitCnt), Use = nameof(MapNullableUInt))]
    [MapProperty(nameof(PlayResultRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.StageData.ToneFlg), nameof(Ac15StageResult.ToneFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.StageData.StageMode), nameof(Ac15StageResult.StageMode), Use = nameof(MapNullableUInt))]
    [MapProperty(nameof(PlayResultRequest.StageData.PlayDan), nameof(Ac15StageResult.PlayDan), Use = nameof(MapPlayDan))]
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

    [MapProperty(nameof(PlayResultRequest.TokkunstageData.BanacoinDatetime), nameof(Ac15TokkunStageData.BanacoinDatetime), Use = nameof(MapString))]
    [UserMapping(Default = true)]
    private static partial Ac15TokkunStageData? MapTokkunStageData(PlayResultRequest.TokkunstageData? data);

    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleStageData? MapBattleStageData(PlayResultRequest.StageData.BattleStageData? data);

    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.BattleNpcData.AcquiredExp), nameof(Ac15BlueBattleNpcData.AcquiredExp), Use = nameof(MapString))]
    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.BattleNpcData.TotalExp), nameof(Ac15BlueBattleNpcData.TotalExp), Use = nameof(MapString))]
    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleNpcData? MapBattleNpcData(PlayResultRequest.StageData.BattleStageData.BattleNpcData? data);

    [MapProperty(nameof(PlayResultRequest.ReleaseBattleData.AryBattletokendatas), nameof(Ac15BlueBattleReleaseData.BattleTokenData), Use = nameof(MapBattleTokenDataList))]
    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleReleaseData? MapBattleReleaseData(PlayResultRequest.ReleaseBattleData? data);

    [UserMapping(Default = true)]
    private static partial Ac15BlueBattleTokenData MapBattleTokenData(PlayResultRequest.ReleaseBattleData.BattleTokenData data);

    [UserMapping(Default = true)]
    private static partial Ac15CompeIdFact MapCompe(PlayResultRequest.StageData.ResultcompeData data);

    [MapProperty(nameof(PlayResultRequest.StageData.AryChallengeIds), nameof(Ac15RedChallengeCompeStageFacts.ChallengeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryUserCompeIds), nameof(Ac15RedChallengeCompeStageFacts.UserCompeIds), Use = nameof(MapCompeList))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryBngCompeIds), nameof(Ac15RedChallengeCompeStageFacts.BngCompeIds), Use = nameof(MapCompeList))]
    [UserMapping(Default = true)]
    private static partial Ac15RedChallengeCompeStageFacts MapChallengeCompeStage(PlayResultRequest.StageData stage);

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

    private static Ac15RedChallengeCompeFacts? MapChallengeCompe(PlayResultRequest request)
        => request.AryStageInfoes.Any(HasChallengeCompeFacts) ? MapChallengeCompeCore(request) : null;

    private static List<Ac15RedChallengeCompeStageFacts> MapChallengeCompeStages(List<PlayResultRequest.StageData> stages)
        => stages.Where(HasChallengeCompeFacts).Select(MapChallengeCompeStage).ToList();

    private static List<Ac15StageResult> MapStages(List<PlayResultRequest.StageData> stages)
        => stages.Select(MapStage).ToList();

    private static List<Ac15CompeIdFact> MapCompeList(List<PlayResultRequest.StageData.ResultcompeData> values)
        => values.Select(MapCompe).ToList();

    private static List<Ac15BlueBattleTokenData> MapBattleTokenDataList(
        List<PlayResultRequest.ReleaseBattleData.BattleTokenData> values)
        => values.Select(MapBattleTokenData).ToList();

    private static bool HasChallengeCompeFacts(PlayResultRequest.StageData stage)
        => stage.AryChallengeIds.Count != 0 || stage.AryUserCompeIds.Count != 0 || stage.AryBngCompeIds.Count != 0;

    private static bool HasCurrentCostume(PlayResultRequest request) => request.AryCurrentCostume is not null;

    private static bool HasDifficultyPlayedCourse(PlayResultRequest request) => request.DifficultyPlayedCourse is not null;

    private static bool HasDifficultyPlayedStar(PlayResultRequest request) => request.DifficultyPlayedStar is not null;

    [UserMapping(Default = true)]
    private static List<uint> MapUIntList(uint[]? values)
        => values?.ToList() ?? [];

    [UserMapping(Default = true)]
    private static uint MapNullableUInt(uint? value)
        => value.GetValueOrDefault();

    private static byte[] MapBytes(byte[]? values)
        => values ?? [];

    private static string MapString(string? value)
        => value ?? string.Empty;

    private static uint? MapPlayDan(uint? value)
        => value is > 0 ? value : null;
}
