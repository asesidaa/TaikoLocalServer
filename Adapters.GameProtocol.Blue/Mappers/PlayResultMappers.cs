using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    [MapPropertyFromSource(nameof(CommonPlayResultData.HasAryCurrentCostume), Use = nameof(HasAryCurrentCostume))]
    [MapPropertyFromSource(nameof(CommonPlayResultData.HasDifficultyPlayedCourse), Use = nameof(HasDifficultyPlayedCourse))]
    [MapPropertyFromSource(nameof(CommonPlayResultData.HasDifficultyPlayedStar), Use = nameof(HasDifficultyPlayedStar))]
    [MapProperty(nameof(PlayResultRequest.Reserved), nameof(CommonPlayResultData.Reserved), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.ContentInfo), nameof(CommonPlayResultData.ContentInfo), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.AryCollaboInfoes), nameof(CommonPlayResultData.AryCollaboInfo))]
    [MapProperty(nameof(PlayResultRequest.AryTokkunstageInfo), nameof(CommonPlayResultData.TokkunStageData), Use = nameof(MapTokkunStageData))]
    [MapProperty(nameof(PlayResultRequest.AryReleaseBattledata), nameof(CommonPlayResultData.BattleReleaseData), Use = nameof(MapBattleReleaseData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.Title))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.TitleplateId))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.CollaborationId))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.DanId))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.SoulGaugeTotal))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.ComboCntTotal))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsNotRecordedDan))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.UraReleaseSongNoes))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.GetGenericInfoNoes))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.DifficultyPlayedSort))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsRandomUsePlay))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.InputMedian))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.InputVariance))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsTokkunPlayResult))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.GhostReleaseData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.GhostUpdatePerfData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.GhostUpdateRankData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.IsBattlePlayResult))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.GetDonpoint))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.RewardPtn))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.RewardProgress))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.DifficultyTutorialFlg))]
    public static partial CommonPlayResultData Map(PlayResultRequest request);

    public static PlayResultResponse Map(uint result)
        => new() { Result = result };

    [MapProperty(nameof(PlayResultRequest.StageData.OptionFlg), nameof(CommonPlayResultData.StageData.OptionFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.StageData.ToneFlg), nameof(CommonPlayResultData.StageData.ToneFlg), Use = nameof(MapBytes))]
    [MapProperty(nameof(PlayResultRequest.StageData.PlayDan), nameof(CommonPlayResultData.StageData.PlayDan), Use = nameof(MapPlayDan))]
    [MapProperty(nameof(PlayResultRequest.StageData.AryBattlestagedata), nameof(CommonPlayResultData.StageData.BattleStageData), Use = nameof(MapBattleStageData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.ScoreRate))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.ScoreRank))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.SupportLevel))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.StarLevel))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsWin))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.ArySectionDatas))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.HitCount))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.GhostStageData))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.NotesPosition))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsVoiceOn))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsSkipOn))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsSkipUse))]
    [MapperIgnoreTarget(nameof(CommonPlayResultData.StageData.IsRandomUseStage))]
    private static partial CommonPlayResultData.StageData MapStage(PlayResultRequest.StageData stage);

    [MapProperty(nameof(PlayResultRequest.CostumeData.Costume1), nameof(CommonPlayResultData.CostumeData.Costume1))]
    [MapProperty(nameof(PlayResultRequest.CostumeData.Costume2), nameof(CommonPlayResultData.CostumeData.Costume2))]
    [MapProperty(nameof(PlayResultRequest.CostumeData.Costume3), nameof(CommonPlayResultData.CostumeData.Costume3))]
    [MapProperty(nameof(PlayResultRequest.CostumeData.Costume4), nameof(CommonPlayResultData.CostumeData.Costume4))]
    [MapProperty(nameof(PlayResultRequest.CostumeData.Costume5), nameof(CommonPlayResultData.CostumeData.Costume5))]
    private static partial CommonPlayResultData.CostumeData MapCostumeData(PlayResultRequest.CostumeData costume);

    [MapProperty(nameof(PlayResultRequest.TokkunstageData.BanacoinDatetime), nameof(CommonPlayResultData.TokkunStageDataDto.BanacoinDatetime), Use = nameof(MapString))]
    private static partial CommonPlayResultData.TokkunStageDataDto MapTokkunStageDataCore(
        PlayResultRequest.TokkunstageData data);

    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.NpcData), nameof(CommonPlayResultData.BattleStageData.NpcData), Use = nameof(MapBattleNpcData))]
    private static partial CommonPlayResultData.BattleStageData MapBattleStageDataCore(
        PlayResultRequest.StageData.BattleStageData data);

    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.BattleNpcData.AcquiredExp), nameof(CommonPlayResultData.BattleNpcData.AcquiredExp), Use = nameof(MapString))]
    [MapProperty(nameof(PlayResultRequest.StageData.BattleStageData.BattleNpcData.TotalExp), nameof(CommonPlayResultData.BattleNpcData.TotalExp), Use = nameof(MapString))]
    private static partial CommonPlayResultData.BattleNpcData MapBattleNpcDataCore(
        PlayResultRequest.StageData.BattleStageData.BattleNpcData data);

    [MapProperty(nameof(PlayResultRequest.ReleaseBattleData.AryBattletokendatas), nameof(CommonPlayResultData.BattleReleaseDataDto.BattleTokenData))]
    private static partial CommonPlayResultData.BattleReleaseDataDto MapBattleReleaseDataCore(
        PlayResultRequest.ReleaseBattleData data);

    private static partial CommonPlayResultData.BattleTokenData MapBattleTokenData(
        PlayResultRequest.ReleaseBattleData.BattleTokenData data);

    private static partial CommonPlayResultData.ResultcompeData MapCompe(
        PlayResultRequest.StageData.ResultcompeData data);

    private static partial CommonPlayResultData.CollaboData MapCollabo(PlayResultRequest.CollaboData data);

    private static CommonPlayResultData.CostumeData MapCostume(PlayResultRequest.CostumeData costume)
        => costume is null ? new CommonPlayResultData.CostumeData() : MapCostumeData(costume);

    private static CommonPlayResultData.TokkunStageDataDto? MapTokkunStageData(
        PlayResultRequest.TokkunstageData data)
        => data is null ? null : MapTokkunStageDataCore(data);

    private static CommonPlayResultData.BattleStageData? MapBattleStageData(
        PlayResultRequest.StageData.BattleStageData data)
        => data is null ? null : MapBattleStageDataCore(data);

    private static CommonPlayResultData.BattleNpcData? MapBattleNpcData(
        PlayResultRequest.StageData.BattleStageData.BattleNpcData data)
        => data is null ? null : MapBattleNpcDataCore(data);

    private static CommonPlayResultData.BattleReleaseDataDto? MapBattleReleaseData(
        PlayResultRequest.ReleaseBattleData data)
        => data is null ? null : MapBattleReleaseDataCore(data);

    [UserMapping(Default = true)]
    private static List<uint> MapUIntList(uint[] values)
        => values is null ? [] : values.ToList();

    [UserMapping(Default = true)]
    private static uint MapNullableUInt(uint? value)
        => value.GetValueOrDefault();

    private static byte[] MapBytes(byte[] values)
        => values is null ? [] : values;

    private static string MapString(string value)
        => value ?? string.Empty;

    private static uint? MapPlayDan(uint? value)
        => value is > 0 ? value : null;

    private static bool HasAryCurrentCostume(PlayResultRequest request)
        => request.AryCurrentCostume is not null;

    private static bool HasDifficultyPlayedCourse(PlayResultRequest request)
        => request.DifficultyPlayedCourse is not null;

    private static bool HasDifficultyPlayedStar(PlayResultRequest request)
        => request.DifficultyPlayedStar is not null;
}
