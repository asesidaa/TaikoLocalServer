namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

public static class PlayResultMappers
{
    public static Ac15PlayResultEnvelope Map(PlayResultDataRequest request)
    {
        var stages = request.AryStageInfoes.Select(MapStage).ToList();
        return new Ac15PlayResultEnvelope(
            Metadata: MapMetadata(request),
            Profile: MapProfile(request),
            Normal: stages.Count == 0 ? null : new Ac15NormalPlayResult(stages),
            Dani: new Ac15DaniPlayResult(request.DanResult.GetValueOrDefault(), ComboCntTotal: 0, stages),
            Tokkun: null,
            BlueBattle: null,
            GreenGhost: MapGreenGhost(request),
            ChallengeCompe: MapChallengeCompe(stages));
    }

    public static PlayResultResponse Map(uint result)
        => new() { Result = result };

    private static Ac15PlayResultMetadata MapMetadata(PlayResultDataRequest request)
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

    private static Ac15ProfileMutationFacts MapProfile(PlayResultDataRequest request)
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

    private static Ac15StageResult MapStage(PlayResultDataRequest.StageData stage)
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
            SupportLevel = stage.SupportLevel,
            MusicCateg = stage.MusicCateg,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            SelectedFolderId = stage.SelectedFolderId,
            StarLevel = stage.StarLevel,
            StageMode = stage.StageMode.GetValueOrDefault(),
            IsPapamama = stage.IsPapamama,
            IsPushed = stage.IsPushed,
            SoulGauge = stage.SoulGauge,
            PlayDan = MapPlayDan(stage.PlayDan),
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge,
            GreenGhostStage = MapGhostStage(stage.GhostStagedata),
            ChallengeIds = MapCompeList(stage.AryChallengeIds),
            UserCompeIds = MapCompeList(stage.AryUserCompeIds),
            BngCompeIds = MapCompeList(stage.AryBngCompeIds)
        };

    private static Ac15GreenGhostPlayResult? MapGreenGhost(PlayResultDataRequest request)
        => request.GhostReleaseData is null && request.GhostUpdatePerfdata is null && request.GhostUpdateRank is null
            ? null
            : new Ac15GreenGhostPlayResult(
                MapGhostRelease(request.GhostReleaseData),
                MapGhostPerf(request.GhostUpdatePerfdata),
                MapGhostRank(request.GhostUpdateRank));

    private static Ac15GreenGhostStageData? MapGhostStage(PlayResultDataRequest.StageData.GhostStageData? data)
        => data is null
            ? null
            : new Ac15GreenGhostStageData
            {
                IsWin = data.IsWin,
                SdCertifiedLevelId = data.SdCertifiedLevelId,
                ArySectionData = data.ArySectionDatas.Select(MapGhostStageSection).ToList()
            };

    private static Ac15GreenGhostStageSectionData MapGhostStageSection(
        PlayResultDataRequest.StageData.GhostStageData.GhostStageSectionData data)
        => new(data.IsWin, data.GoodCnt, data.OkCnt, data.NgCnt, data.PoundCnt);

    private static Ac15GreenGhostReleaseData? MapGhostRelease(PlayResultDataRequest.UpdateGhostInfoData? data)
        => data is null
            ? null
            : new Ac15GreenGhostReleaseData
            {
                ReleaseInfoId = MapUIntList(data.ReleaseInfoIds),
                AryTokendata = data.AryTokendatas.Select(MapGhostToken).ToList()
            };

    private static Ac15GreenGhostTokenData MapGhostToken(PlayResultDataRequest.UpdateGhostInfoData.GhostTokenData data)
        => new(data.TokenId, data.TokenValue);

    private static Ac15GreenGhostPerfData? MapGhostPerf(PlayResultDataRequest.UpdateGhostPerfData? data)
        => data is null ? null : new Ac15GreenGhostPerfData(data.InputMedian, data.InputVariance);

    private static Ac15GreenGhostRankData? MapGhostRank(PlayResultDataRequest.UpdateGhostRankData? data)
        => data is null
            ? null
            : new Ac15GreenGhostRankData
            {
                RankId = data.RankId,
                WinPoint = data.WinPoint,
                CertifiedLevelId = data.CertifiedLevelId,
                AryWinningsData = data.AryWinningsDatas.Select(MapGhostWinnings).ToList()
            };

    private static Ac15GreenGhostWinningsData MapGhostWinnings(
        PlayResultDataRequest.UpdateGhostRankData.UpdateGhostWinningsData data)
        => new(data.LevelId, data.Winnings);

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

    private static Ac15CostumeFacts MapCostume(PlayResultDataRequest.CostumeData? costume)
        => costume is null
            ? Ac15CostumeFacts.Empty
            : new Ac15CostumeFacts(
                costume.Costume1.GetValueOrDefault(),
                costume.Costume2.GetValueOrDefault(),
                costume.Costume3.GetValueOrDefault(),
                costume.Costume4.GetValueOrDefault(),
                costume.Costume5.GetValueOrDefault());

    private static List<Ac15CompeIdFact> MapCompeList(IEnumerable<PlayResultDataRequest.StageData.ResultcompeData>? values)
        => values?.Select(data => new Ac15CompeIdFact(data.CompeId, data.TrackNo)).ToList() ?? [];

    private static List<uint> MapUIntList(IEnumerable<uint>? values)
        => values?.ToList() ?? [];

    private static byte[] MapBytes(byte[]? values)
        => values is null ? [] : values;

    private static uint? MapPlayDan(uint? value)
        => value is > 0 ? value : null;
}
