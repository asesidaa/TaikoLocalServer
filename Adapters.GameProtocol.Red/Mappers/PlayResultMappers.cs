namespace TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;

public static class PlayResultMappers
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
            BlueBattle: null,
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
            GetDonpoint = request.GetDonpoint.GetValueOrDefault(),
            RewardPtn = request.RewardPtn,
            RewardProgress = request.RewardProgress,
            DifficultyTutorialFlg = request.DifficultyTutorialFlg,
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
            PlayResult = stage.PlayResult,
            PlayScore = stage.PlayScore,
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
            StageMode = stage.StageMode,
            IsPapamama = stage.IsPapamama,
            IsPushed = stage.IsPushed,
            SoulGauge = stage.SoulGauge,
            PlayDan = MapPlayDan(stage.PlayDan),
            ChallengeIds = MapCompeList(stage.AryChallengeIds),
            UserCompeIds = MapCompeList(stage.AryUserCompeIds),
            BngCompeIds = MapCompeList(stage.AryBngCompeIds)
        };

    private static Ac15TokkunPlayResult? MapTokkun(PlayResultRequest request)
        => request.TokkunTutorialFlg is null && request.AryTokkunstageInfo is null
            ? null
            : new Ac15TokkunPlayResult(request.TokkunTutorialFlg, MapTokkunStageData(request.AryTokkunstageInfo));

    private static Ac15TokkunStageData? MapTokkunStageData(PlayResultRequest.TokkunstageData? data)
        => data is null
            ? null
            : new Ac15TokkunStageData(
                data.BanacoinDatetime ?? string.Empty,
                data.TokkunSongCnt,
                MapUIntList(data.TookunSongnoes),
                data.TokkunSpeedchangeCnt,
                data.TokkunAutoplayCnt,
                data.TokkunJumpCnt);

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

    private static Ac15CostumeFacts MapCostume(PlayResultRequest.CostumeData? costume)
        => costume is null
            ? Ac15CostumeFacts.Empty
            : new Ac15CostumeFacts(
                costume.Costume1.GetValueOrDefault(),
                costume.Costume2.GetValueOrDefault(),
                costume.Costume3.GetValueOrDefault(),
                costume.Costume4.GetValueOrDefault(),
                costume.Costume5.GetValueOrDefault());

    private static List<Ac15CompeIdFact> MapCompeList(IEnumerable<PlayResultRequest.StageData.ResultcompeData>? values)
        => values?.Select(data => new Ac15CompeIdFact(data.CompeId, data.TrackNo)).ToList() ?? [];

    private static List<uint> MapUIntList(IEnumerable<uint>? values)
        => values?.ToList() ?? [];

    private static byte[] MapBytes(byte[]? values)
        => values is null ? [] : values;

    private static uint? MapPlayDan(uint value)
        => value > 0 ? value : null;
}
