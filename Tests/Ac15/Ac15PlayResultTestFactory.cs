using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

internal static class Ac15PlayResultTestFactory
{
    public static UpdateAc15PlayResultCommand FromCommon(
        uint baid,
        GameEra era,
        CommonPlayResultData data)
    {
        var stages = data.AryStageInfoes.Select(MapStage).ToList();
        var profile = Ac15ProfileMutationFacts.Empty with
        {
            AreaCode = data.AreaCode,
            GetDonmedal = data.GetDonmedal,
            GetKatsumedal = data.GetKatsumedal,
            GetDonpoint = data.GetDonpoint,
            RewardPtn = data.RewardPtn,
            RewardProgress = data.RewardProgress,
            DifficultyTutorialFlg = data.DifficultyTutorialFlg,
            ItemshopTutorialFlg = data.ItemshopTutorialFlg,
            WaiwaiTutorialFlg = data.WaiwaiTutorialFlg,
            IsDevil = data.IsDevil,
            IsExplain = data.IsExplain,
            HasDifficultyPlayedCourse = data.HasDifficultyPlayedCourse,
            DifficultyPlayedCourse = data.DifficultyPlayedCourse,
            HasDifficultyPlayedStar = data.HasDifficultyPlayedStar,
            DifficultyPlayedStar = data.DifficultyPlayedStar,
            HasAryCurrentCostume = data.HasAryCurrentCostume,
            AryCurrentCostume = new Ac15CostumeFacts(
                data.AryCurrentCostume.Costume1,
                data.AryCurrentCostume.Costume2,
                data.AryCurrentCostume.Costume3,
                data.AryCurrentCostume.Costume4,
                data.AryCurrentCostume.Costume5),
            ReleaseSongNoes = data.ReleaseSongNoes,
            GetToneNoes = data.GetToneNoes,
            GetCostumeNo1s = data.GetCostumeNo1s,
            GetCostumeNo2s = data.GetCostumeNo2s,
            GetCostumeNo3s = data.GetCostumeNo3s,
            GetCostumeNo4s = data.GetCostumeNo4s,
            GetCostumeNo5s = data.GetCostumeNo5s,
            GetTitleNoes = data.GetTitleNoes
        };

        return Command(
            baid,
            era,
            playMode: data.PlayMode,
            playDatetime: data.PlayDatetime,
            profile: profile,
            stages: stages,
            dani: new Ac15DaniPlayResult(data.DanResult, data.ComboCntTotal, stages),
            tokkun: MapTokkun(data),
            battle: MapBattle(data, stages),
            ghost: MapGhost(data));
    }

    public static UpdateAc15PlayResultCommand Command(
        uint baid,
        GameEra era,
        uint playMode = 0,
        string playDatetime = "20260608120000",
        Ac15ProfileMutationFacts? profile = null,
        List<Ac15StageResult>? stages = null,
        Ac15DaniPlayResult? dani = null,
        Ac15TokkunPlayResult? tokkun = null,
        Ac15BlueBattlePlayResult? battle = null,
        Ac15GreenGhostPlayResult? ghost = null)
    {
        var stageList = stages ?? [];
        var metadata = new Ac15PlayResultMetadata(
            Baid: baid,
            ChassisId: "268410000000",
            ShopId: "JPN0JPN0123",
            PlayDatetime: playDatetime,
            IsRight: false,
            CardType: 1,
            IsTwoPlayers: false,
            PlayMode: playMode,
            AreaCode: profile?.AreaCode ?? 1,
            Reserved: [],
            Accesstoken: string.Empty,
            ContentInfo: []);

        var envelope = new Ac15PlayResultEnvelope(
            metadata,
            profile ?? Ac15ProfileMutationFacts.Empty,
            stageList.Count == 0 ? null : new Ac15NormalPlayResult(stageList),
            dani ?? new Ac15DaniPlayResult(DanResult: 0, ComboCntTotal: 0, Stages: stageList),
            tokkun,
            battle,
            ghost);

        return new UpdateAc15PlayResultCommand(baid, era, envelope);
    }

    private static Ac15StageResult MapStage(CommonPlayResultData.StageData stage)
        => new()
        {
            SongNo = stage.SongNo,
            Level = stage.Level,
            PlayResult = stage.PlayResult,
            PlayScore = stage.PlayScore,
            ScoreRate = stage.ScoreRate,
            ScoreRank = stage.ScoreRank,
            GoodCnt = stage.GoodCnt,
            OkCnt = stage.OkCnt,
            NgCnt = stage.NgCnt,
            PoundCnt = stage.PoundCnt,
            ComboCnt = stage.ComboCnt,
            HitCnt = stage.HitCnt,
            OptionFlg = stage.OptionFlg,
            ToneFlg = stage.ToneFlg,
            SupportLevel = stage.SupportLevel,
            MusicCateg = stage.MusicCateg,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            SelectedFolderId = stage.SelectedFolderId,
            StarLevel = stage.StarLevel,
            IsWin = stage.IsWin,
            StageMode = stage.StageMode,
            IsPapamama = stage.IsPapamama,
            IsPushed = stage.IsPushed,
            SoulGauge = stage.SoulGauge,
            HitCount = stage.HitCount,
            PlayDan = stage.PlayDan,
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge,
            BlueBattleStage = MapBattleStage(stage.BattleStageData),
            GreenGhostStage = MapGhostStage(stage.GhostStageData),
            ChallengeIds = MapCompe(stage.AryChallengeIds),
            UserCompeIds = MapCompe(stage.AryUserCompeIds),
            BngCompeIds = MapCompe(stage.AryBngCompeIds),
            AiSectionData = stage.ArySectionDatas
                .Select(section => new Ac15AiStageSectionData(
                    section.IsWin,
                    section.Crown,
                    section.Score,
                    section.GoodCnt,
                    section.OkCnt,
                    section.NgCnt,
                    section.PoundCnt))
                .ToList()
        };

    private static Ac15TokkunPlayResult? MapTokkun(CommonPlayResultData data)
        => data.PlayMode == (uint)PlayMode.Tokkun || data.TokkunTutorialFlg is not null || data.TokkunStageData is not null
            ? new Ac15TokkunPlayResult(data.TokkunTutorialFlg, MapTokkunStage(data.TokkunStageData))
            : null;

    private static Ac15TokkunStageData? MapTokkunStage(CommonPlayResultData.TokkunStageDataDto? data)
        => data is null
            ? null
            : new Ac15TokkunStageData(
                data.BanacoinDatetime,
                data.TokkunSongCnt,
                data.TookunSongnoes,
                data.TokkunSpeedchangeCnt,
                data.TokkunAutoplayCnt,
                data.TokkunJumpCnt);

    private static Ac15BlueBattlePlayResult? MapBattle(CommonPlayResultData data, List<Ac15StageResult> stages)
        => data.IsBattlePlayResult || data.BattleReleaseData is not null || stages.Any(stage => stage.BlueBattleStage is not null)
            ? new Ac15BlueBattlePlayResult(MapBattleRelease(data.BattleReleaseData), stages, data.GetDonmedal)
            : null;

    private static Ac15BlueBattleStageData? MapBattleStage(CommonPlayResultData.BattleStageData? data)
        => data is null
            ? null
            : new Ac15BlueBattleStageData
            {
                SupportLv = data.SupportLv,
                BattleStageId = data.BattleStageId,
                NpcData = MapBattleNpc(data.NpcData),
                KillCnt = data.KillCnt,
                BossLife = data.BossLife,
                TotalDamage = data.TotalDamage,
                CriticalCnt = data.CriticalCnt,
                SpecialMoveCnt = data.SpecialMoveCnt
            };

    private static Ac15BlueBattleNpcData? MapBattleNpc(CommonPlayResultData.BattleNpcData? data)
        => data is null
            ? null
            : new Ac15BlueBattleNpcData
            {
                NpcId = data.NpcId,
                AcquiredExp = data.AcquiredExp,
                TotalExp = data.TotalExp,
                Dpn = data.Dpn,
                NpcCostumeId = data.NpcCostumeId,
                SpecialId1 = data.SpecialId1,
                SpecialId2 = data.SpecialId2,
                SpecialId3 = data.SpecialId3,
                BondsLv = data.BondsLv
            };

    private static Ac15BlueBattleReleaseData? MapBattleRelease(CommonPlayResultData.BattleReleaseDataDto? data)
        => data is null
            ? null
            : new Ac15BlueBattleReleaseData
            {
                ReleaseInfoIds = data.ReleaseInfoIds,
                ReleaseBattleStageIds = data.ReleaseBattleStageIds,
                ReleaseNpcIds = data.ReleaseNpcIds,
                ReleaseNpcCostumeIds = data.ReleaseNpcCostumeIds,
                ReleaseNpcSpecialIds = data.ReleaseNpcSpecialIds,
                BattleTokenData = data.BattleTokenData.Select(token => new Ac15BlueBattleTokenData(token.TokenId, token.TokenValue)).ToList(),
                AssignNextStageId = data.AssignNextStageId
            };

    private static Ac15GreenGhostPlayResult? MapGhost(CommonPlayResultData data)
        => data.GhostReleaseData is null && data.GhostUpdatePerfData is null && data.GhostUpdateRankData is null
            ? null
            : new Ac15GreenGhostPlayResult(
                MapGhostRelease(data.GhostReleaseData),
                data.GhostUpdatePerfData is null ? null : new Ac15GreenGhostPerfData(data.GhostUpdatePerfData.InputMedian, data.GhostUpdatePerfData.InputVariance),
                MapGhostRank(data.GhostUpdateRankData));

    private static Ac15GreenGhostStageData? MapGhostStage(CommonPlayResultData.GhostStageData? data)
        => data is null
            ? null
            : new Ac15GreenGhostStageData
            {
                IsWin = data.IsWin,
                SdCertifiedLevelId = data.SdCertifiedLevelId,
                ArySectionData = data.ArySectionData
                    .Select(section => new Ac15GreenGhostStageSectionData(
                        section.IsWin,
                        section.GoodCnt,
                        section.OkCnt,
                        section.NgCnt,
                        section.PoundCnt))
                    .ToList()
            };

    private static Ac15GreenGhostReleaseData? MapGhostRelease(CommonPlayResultData.UpdateGhostInfoData? data)
        => data is null
            ? null
            : new Ac15GreenGhostReleaseData
            {
                ReleaseInfoId = data.ReleaseInfoId,
                AryTokendata = data.AryTokendata.Select(token => new Ac15GreenGhostTokenData(token.TokenId, token.TokenValue)).ToList()
            };

    private static Ac15GreenGhostRankData? MapGhostRank(CommonPlayResultData.UpdateGhostRankData? data)
        => data is null
            ? null
            : new Ac15GreenGhostRankData
            {
                RankId = data.RankId,
                WinPoint = data.WinPoint,
                CertifiedLevelId = data.CertifiedLevelId,
                AryWinningsData = data.AryWinningsData.Select(winning => new Ac15GreenGhostWinningsData(winning.LevelId, winning.Winnings)).ToList()
            };

    private static List<Ac15CompeIdFact> MapCompe(IEnumerable<CommonPlayResultData.ResultcompeData> values)
        => values.Select(value => new Ac15CompeIdFact(value.CompeId, value.TrackNo)).ToList();
}
