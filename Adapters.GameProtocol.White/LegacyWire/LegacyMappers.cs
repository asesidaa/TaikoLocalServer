using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class LegacyBaidResponseMapper
{
    [MapProperty(nameof(Ac15BaidIdentity.MyDonName), nameof(BAIDResponse.MydonName))]
    [MapperIgnoreSource(nameof(Ac15BaidIdentity.MyDonNameLanguage))]
    public static partial void Apply(Ac15BaidIdentity source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidProfile.SelectedCostume), nameof(BAIDResponse.AryCostumedata), Use = nameof(MapCostumeData))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.TitlePlateId))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.IsAutoCostumeOn))]
    [MapperIgnoreSource(nameof(Ac15BaidProfile.LastPlayDatetime))]
    public static partial void Apply(Ac15BaidProfile source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg1), nameof(BAIDResponse.CostumeFlg1), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg2), nameof(BAIDResponse.CostumeFlg2), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg3), nameof(BAIDResponse.CostumeFlg3), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg4), nameof(BAIDResponse.CostumeFlg4), Use = nameof(MapCostumeFlag))]
    [MapProperty(nameof(Ac15BaidCostumeFlags.CostumeFlg5), nameof(BAIDResponse.CostumeFlg5), Use = nameof(MapCostumeFlag))]
    public static partial void Apply(Ac15BaidCostumeFlags source, [MappingTarget] BAIDResponse response);

    [MapProperty(nameof(Ac15BaidDan.GotDanFlg), nameof(BAIDResponse.GotDanFlg), Use = nameof(MapDanFlag))]
    [MapperIgnoreSource(nameof(Ac15BaidDan.GotDanExtraFlg))]
    public static partial void Apply(Ac15BaidDan source, [MappingTarget] BAIDResponse response);

    [MapperIgnoreSource(nameof(Ac15BaidCompatibility.PersonId))]
    [MapperIgnoreSource(nameof(Ac15BaidCompatibility.WaiwaiTutorialFlg))]
    public static partial void Apply(Ac15BaidCompatibility source, [MappingTarget] BAIDResponse response);

    public static partial void Apply(Ac15BaidReward source, [MappingTarget] BAIDResponse response);

    private static partial BAIDResponse.CostumeData MapCostumeData(Ac15CostumeFacts values);

    private static byte[] MapCostumeFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.White.Limits.CostumeFlagBytes);

    private static byte[] MapDanFlag(byte[]? value)
        => Ac15ProtocolBytes.FixedOrZero(value, Ac15EraProfiles.White.Limits.DanFlagBytes);
}

[Mapper]
public static partial class LegacyFolderDataMappers
{
    [MapProperty(nameof(CommonGetFolderResponse.AryEventfolderDatas), nameof(GetfolderResponse.AryEventfolderDatas))]
    public static partial GetfolderResponse Map(CommonGetFolderResponse common);

    private static partial GetfolderResponse.EventfolderData MapEventFolderData(EventFolderData folder);
}

[Mapper]
public static partial class LegacyGetTelopMappers
{
    [MapProperty(nameof(CommonGetTelopResponse.StartDatetime), nameof(GettelopResponse.StartDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.EndDatetime), nameof(GettelopResponse.EndDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.Telop), nameof(GettelopResponse.Telop), Use = nameof(MapPresentString))]
    public static partial GettelopResponse Map(CommonGetTelopResponse common);

    private static string MapPresentString(string? value) => string.IsNullOrEmpty(value) ? null! : value;
}

[Mapper]
public static partial class LegacyInitialDataMappers
{
    [MapProperty(nameof(CommonInitialDataCheckResponse.DefaultSongFlg), nameof(InitialdatacheckResponse.HashDefaultSongFlg))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AchievementSongBit), nameof(InitialdatacheckResponse.HashMainichidojoAll))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.UraReleaseBit), nameof(InitialdatacheckResponse.HashMainichidojoRare))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryTelopDatas), nameof(InitialdatacheckResponse.AryTelopDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryEventFolderDatas), nameof(InitialdatacheckResponse.AryEventfolderDatas))]
    [MapProperty(nameof(CommonInitialDataCheckResponse.AryTaikojukuDatas), nameof(InitialdatacheckResponse.AryTaikojukuDatas))]
    [MapperIgnoreSource(nameof(CommonInitialDataCheckResponse.AryLegaltermsDatas))]
    public static partial InitialdatacheckResponse Map(CommonInitialDataCheckResponse common);

    private static partial InitialdatacheckResponse.InformationData MapInformation(
        CommonInitialDataCheckResponse.InformationData common);
}

[Mapper(
    AllowNullPropertyAssignment = false,
    ThrowOnMappingNullMismatch = false,
    ThrowOnPropertyMappingNullMismatch = false)]
public static partial class LegacyPlayResultMappers
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
    [MapPropertyFromSource(nameof(Ac15ProfileMutationFacts.HasAryCurrentCostume), Use = nameof(HasCurrentCostume))]
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
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRate))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.ScoreRank))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.SupportLevel))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.StarLevel))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.IsWin))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.HitCount))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.WaiwaiResult))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.WaiwaiGauge))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.BlueBattleStage))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.GreenGhostStage))]
    [MapperIgnoreTarget(nameof(Ac15StageResult.AiSectionData))]
    [UserMapping(Default = true)]
    private static partial Ac15StageResult MapStage(PlayResultRequest.StageData stage);

    [UserMapping(Default = true)]
    private static partial Ac15CostumeFacts MapCostumeData(PlayResultRequest.CostumeData costume);

    [UserMapping(Default = true)]
    private static partial Ac15CompeIdFact MapCompe(PlayResultRequest.StageData.ResultcompeData data);

    private static Ac15NormalPlayResult? MapNormal(PlayResultRequest request)
        => request.AryStageInfoes.Count == 0 ? null : MapNormalCore(request);

    private static List<Ac15StageResult> MapStages(List<PlayResultRequest.StageData> stages)
        => stages.Select(MapStage).ToList();

    private static List<Ac15CompeIdFact> MapCompeList(List<PlayResultRequest.StageData.ResultcompeData> values)
        => values.Select(MapCompe).ToList();

    private static bool HasCurrentCostume(PlayResultRequest request) => request.AryCurrentCostume is not null;

    [UserMapping(Default = true)]
    private static List<uint> MapUIntList(uint[]? values)
        => values?.ToList() ?? [];

}

[Mapper]
public static partial class LegacyRecommendMappers
{
    [MapProperty(nameof(CommonRecommendResponse.RecommendBestSong), nameof(RecommendResponse.RecommendBestSongs))]
    public static partial RecommendResponse Map(CommonRecommendResponse common);
}

[Mapper]
public static partial class LegacySelfBestMappers
{
    [MapProperty(nameof(CommonSelfBestResponse.ArySelfbestScores), nameof(SelfBestResponse.ArySelfbestScores))]
    [MapProperty(nameof(CommonSelfBestResponse.AryShinSelfbestScores), nameof(SelfBestResponse.AryShinSelfbestScores))]
    public static partial SelfBestResponse Map(CommonSelfBestResponse common);

    private static partial SelfBestResponse.SelfBestData MapSelfBestData(CommonSelfBestResponse.SelfBestData row);
}

[Mapper]
public static partial class LegacyTaikojukuMappers
{
    [MapProperty(nameof(CommonTaikojukuResponse.Packs), nameof(TaikojukuResponse.AryJukupackDatas))]
    public static partial TaikojukuResponse Map(CommonTaikojukuResponse common);

    [MapProperty(nameof(CommonTaikojukuResponse.Pack.Songs), nameof(TaikojukuResponse.JukupackData.AryJukusongDatas))]
    private static partial TaikojukuResponse.JukupackData MapPack(
        CommonTaikojukuResponse.Pack pack);

    [MapProperty(nameof(CommonTaikojukuResponse.Song.Level), nameof(TaikojukuResponse.JukupackData.JukusongData.Level), Use = nameof(MapTaikojukuDifficulty))]
    private static partial TaikojukuResponse.JukupackData.JukusongData MapSong(
        CommonTaikojukuResponse.Song song);

    private static uint MapTaikojukuDifficulty(Difficulty value) => Ac15Difficulty.ToProtocol(value);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class LegacyUserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataProfileCounters source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataDisplaySettings.DispTaikojukuDan), nameof(UserDataResponse.DispTaikojukuDan), Use = nameof(MapDispTaikojukuDan))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedCourse))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedStar))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsChallengeCompe))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsTojiru))]
    public static partial void Apply(Ac15UserDataDisplaySettings source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataModeFlags source, [MappingTarget] UserDataResponse response);

    [MapperIgnoreSource(nameof(Ac15UserDataTutorial.TokkunTutorialFlg))]
    [MapperIgnoreSource(nameof(Ac15UserDataTutorial.DifficultyTutorialFlg))]
    public static partial void Apply(Ac15UserDataTutorial source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataReward source, [MappingTarget] UserDataResponse response);

    private static uint? MapDispTaikojukuDan(uint value) => value is >= 1 and <= 25 ? value : 1u;

    private static uint[] MapRecommendBestSongs(List<uint> value) => value.ToArray();
}
