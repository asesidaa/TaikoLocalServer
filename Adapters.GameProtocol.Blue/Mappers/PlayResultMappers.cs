using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static Ac15PlayResultEnvelope Map(PlayResultRequest request)
    {
        var stages = request.AryStageInfoes.Select(MapStage).ToList();
        return new Ac15PlayResultEnvelope(
            Metadata: MapMetadata(request),
            Profile: MapProfile(request),
            Normal: stages.Count == 0 ? null : new Ac15NormalPlayResult(stages),
            Dani: new Ac15DaniPlayResult(request.DanResult.GetValueOrDefault(), ComboCntTotal: 0, stages),
            Tokkun: MapTokkun(request),
            BlueBattle: MapBlueBattle(request, stages),
            GreenGhost: null,
            ChallengeCompe: MapChallengeCompe(stages));
    }

    public static PlayResultResponse Map(uint result)
        => new() { Result = result };

    private static Ac15PlayResultMetadata MapMetadata(PlayResultRequest request)
        => new(
            request.Baid,
            request.ChassisId ?? string.Empty,
            request.ShopId ?? string.Empty,
            request.PlayDatetime ?? string.Empty,
            request.IsRight,
            request.CardType,
            request.IsTwoPlayers,
            request.PlayMode,
            request.AreaCode,
            MapBytes(request.Reserved),
            request.Accesstoken ?? string.Empty,
            MapBytes(request.ContentInfo));

    private static Ac15ProfileMutationFacts MapProfile(PlayResultRequest request)
        => Ac15ProfileMutationFacts.Empty with
        {
            AreaCode = request.AreaCode,
            GetDonmedal = request.GetDonmedal,
            GetKatsumedal = request.GetKatsumedal,
            ItemshopTutorialFlg = request.ItemshopTutorialFlg,
            WaiwaiTutorialFlg = request.WaiwaiTutorialFlg,
            IsDevil = request.IsDevil,
            IsExplain = request.IsExplain,
            HasDifficultyPlayedCourse = request.DifficultyPlayedCourse is not null,
            DifficultyPlayedCourse = request.DifficultyPlayedCourse.GetValueOrDefault(),
            HasDifficultyPlayedStar = request.DifficultyPlayedStar is not null,
            DifficultyPlayedStar = request.DifficultyPlayedStar.GetValueOrDefault(),
            HasAryCurrentCostume = request.AryCurrentCostume is not null,
            AryCurrentCostume = MapCostume(request.AryCurrentCostume),
            ReleaseSongNoes = MapUIntList(request.ReleaseSongNoes),
            GetToneNoes = MapUIntList(request.GetToneNoes),
            GetCostumeNo1s = MapUIntList(request.GetCostumeNo1s),
            GetCostumeNo2s = MapUIntList(request.GetCostumeNo2s),
            GetCostumeNo3s = MapUIntList(request.GetCostumeNo3s),
            GetCostumeNo4s = MapUIntList(request.GetCostumeNo4s),
            GetCostumeNo5s = MapUIntList(request.GetCostumeNo5s),
            GetTitleNoes = MapUIntList(request.GetTitleNoes)
        };

    private static Ac15StageResult MapStage(PlayResultRequest.StageData stage)
        => new()
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
            OptionFlg = MapBytes(stage.OptionFlg),
            ToneFlg = MapBytes(stage.ToneFlg),
            MusicCateg = stage.MusicCateg,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            SelectedFolderId = stage.SelectedFolderId,
            StageMode = stage.StageMode.GetValueOrDefault(),
            IsPapamama = stage.IsPapamama,
            IsPushed = stage.IsPushed,
            SoulGauge = stage.SoulGauge,
            PlayDan = MapPlayDan(stage.PlayDan),
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge,
            BlueBattleStage = MapBattleStageData(stage.AryBattlestagedata),
            ChallengeIds = MapCompeList(stage.AryChallengeIds),
            UserCompeIds = MapCompeList(stage.AryUserCompeIds),
            BngCompeIds = MapCompeList(stage.AryBngCompeIds)
        };

    private static partial Ac15CostumeFacts MapCostumeData(PlayResultRequest.CostumeData costume);

    [MapProperty(nameof(PlayResultRequest.TokkunstageData.BanacoinDatetime), nameof(Ac15TokkunStageData.BanacoinDatetime), Use = nameof(MapString))]
    private static partial Ac15TokkunStageData MapTokkunStageDataCore(PlayResultRequest.TokkunstageData data);

    private static Ac15CostumeFacts MapCostume(PlayResultRequest.CostumeData? costume)
        => costume is null ? Ac15CostumeFacts.Empty : MapCostumeData(costume);

    private static Ac15TokkunPlayResult? MapTokkun(PlayResultRequest request)
        => request.TokkunTutorialFlg is null && request.AryTokkunstageInfo is null
            ? null
            : new Ac15TokkunPlayResult(request.TokkunTutorialFlg, MapTokkunStageData(request.AryTokkunstageInfo));

    private static Ac15BlueBattlePlayResult? MapBlueBattle(PlayResultRequest request, List<Ac15StageResult> stages)
        => request.AryReleaseBattledata is null && stages.All(stage => stage.BlueBattleStage is null)
            ? null
            : new Ac15BlueBattlePlayResult(MapBattleReleaseData(request.AryReleaseBattledata), stages, request.GetDonmedal);

    private static Ac15RedChallengeCompeFacts? MapChallengeCompe(List<Ac15StageResult> stages)
    {
        var facts = stages
            .Where(stage => stage.ChallengeIds.Count != 0 || stage.UserCompeIds.Count != 0 || stage.BngCompeIds.Count != 0)
            .Select(stage => new Ac15RedChallengeCompeStageFacts(
                stage.SongNo,
                stage.ChallengeIds,
                stage.UserCompeIds,
                stage.BngCompeIds))
            .ToList();
        return facts.Count == 0 ? null : new Ac15RedChallengeCompeFacts(facts);
    }

    private static Ac15TokkunStageData? MapTokkunStageData(PlayResultRequest.TokkunstageData data)
        => data is null ? null : MapTokkunStageDataCore(data);

    private static Ac15BlueBattleStageData? MapBattleStageData(PlayResultRequest.StageData.BattleStageData? data)
        => data is null
            ? null
            : new Ac15BlueBattleStageData
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

    private static Ac15BlueBattleNpcData? MapBattleNpcData(PlayResultRequest.StageData.BattleStageData.BattleNpcData? data)
        => data is null
            ? null
            : new Ac15BlueBattleNpcData
            {
                NpcId = data.NpcId,
                AcquiredExp = MapString(data.AcquiredExp),
                TotalExp = MapString(data.TotalExp),
                Dpn = data.Dpn,
                NpcCostumeId = data.NpcCostumeId,
                SpecialId1 = data.SpecialId1,
                SpecialId2 = data.SpecialId2,
                SpecialId3 = data.SpecialId3,
                BondsLv = data.BondsLv
            };

    private static Ac15BlueBattleReleaseData? MapBattleReleaseData(PlayResultRequest.ReleaseBattleData? data)
        => data is null
            ? null
            : new Ac15BlueBattleReleaseData
            {
                ReleaseInfoIds = MapUIntList(data.ReleaseInfoIds),
                ReleaseBattleStageIds = MapUIntList(data.ReleaseBattleStageIds),
                ReleaseNpcIds = MapUIntList(data.ReleaseNpcIds),
                ReleaseNpcCostumeIds = MapUIntList(data.ReleaseNpcCostumeIds),
                ReleaseNpcSpecialIds = MapUIntList(data.ReleaseNpcSpecialIds),
                BattleTokenData = data.AryBattletokendatas?.Select(MapBattleTokenData).ToList() ?? [],
                AssignNextStageId = data.AssignNextStageId
            };

    private static Ac15BlueBattleTokenData MapBattleTokenData(PlayResultRequest.ReleaseBattleData.BattleTokenData data)
        => new(data.TokenId, data.TokenValue);

    private static List<Ac15CompeIdFact> MapCompeList(IEnumerable<PlayResultRequest.StageData.ResultcompeData>? values)
        => values?.Select(data => new Ac15CompeIdFact(data.CompeId, data.TrackNo)).ToList() ?? [];

    [UserMapping(Default = true)]
    private static List<uint> MapUIntList(uint[] values)
        => values is null ? [] : values.ToList();

    [UserMapping(Default = true)]
    private static uint MapNullableUInt(uint? value)
        => value.GetValueOrDefault();

    private static byte[] MapBytes(byte[]? values)
        => values is null ? [] : values;

    private static string MapString(string? value)
        => value ?? string.Empty;

    private static uint? MapPlayDan(uint? value)
        => value is > 0 ? value : null;
}
