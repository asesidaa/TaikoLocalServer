namespace TaikoLocalServer.Application.Dtos.Ac15;

internal static class Ac15PlayResultCommonBridge
{
    public static CommonPlayResultData ToCommon(Ac15PlayResultEnvelope envelope)
    {
        var stages = envelope.Normal?.Stages
                     ?? envelope.BlueBattle?.Stages
                     ?? envelope.Dani?.Stages
                     ?? [];

        return new CommonPlayResultData
        {
            Baid = envelope.Metadata.Baid,
            ChassisId = envelope.Metadata.ChassisId,
            ShopId = envelope.Metadata.ShopId,
            PlayDatetime = envelope.Metadata.PlayDatetime,
            IsRight = envelope.Metadata.IsRight,
            CardType = envelope.Metadata.CardType,
            IsTwoPlayers = envelope.Metadata.IsTwoPlayers,
            PlayMode = envelope.Metadata.PlayMode,
            AreaCode = envelope.Profile.AreaCode,
            Reserved = envelope.Metadata.Reserved,
            Accesstoken = envelope.Metadata.Accesstoken,
            ContentInfo = envelope.Metadata.ContentInfo,
            GetDonmedal = envelope.Profile.GetDonmedal,
            GetKatsumedal = envelope.Profile.GetKatsumedal,
            GetDonpoint = envelope.Profile.GetDonpoint,
            RewardPtn = envelope.Profile.RewardPtn,
            RewardProgress = envelope.Profile.RewardProgress,
            DifficultyTutorialFlg = envelope.Profile.DifficultyTutorialFlg,
            ItemshopTutorialFlg = envelope.Profile.ItemshopTutorialFlg,
            WaiwaiTutorialFlg = envelope.Profile.WaiwaiTutorialFlg,
            IsDevil = envelope.Profile.IsDevil,
            IsExplain = envelope.Profile.IsExplain,
            HasDifficultyPlayedCourse = envelope.Profile.HasDifficultyPlayedCourse,
            DifficultyPlayedCourse = envelope.Profile.DifficultyPlayedCourse,
            HasDifficultyPlayedStar = envelope.Profile.HasDifficultyPlayedStar,
            DifficultyPlayedStar = envelope.Profile.DifficultyPlayedStar,
            HasAryCurrentCostume = envelope.Profile.HasAryCurrentCostume,
            AryCurrentCostume = ToCommonCostume(envelope.Profile.AryCurrentCostume),
            ReleaseSongNoes = envelope.Profile.ReleaseSongNoes,
            GetToneNoes = envelope.Profile.GetToneNoes,
            GetCostumeNo1s = envelope.Profile.GetCostumeNo1s,
            GetCostumeNo2s = envelope.Profile.GetCostumeNo2s,
            GetCostumeNo3s = envelope.Profile.GetCostumeNo3s,
            GetCostumeNo4s = envelope.Profile.GetCostumeNo4s,
            GetCostumeNo5s = envelope.Profile.GetCostumeNo5s,
            GetTitleNoes = envelope.Profile.GetTitleNoes,
            DanResult = envelope.Dani?.DanResult ?? 0,
            ComboCntTotal = envelope.Dani?.ComboCntTotal ?? 0,
            TokkunTutorialFlg = envelope.Tokkun?.TutorialFlg,
            TokkunStageData = envelope.Tokkun?.StageData is { } tokkun ? ToCommonTokkun(tokkun) : null,
            BattleReleaseData = envelope.BlueBattle?.ReleaseData is { } battleRelease
                ? ToCommonBattleRelease(battleRelease)
                : null,
            GhostReleaseData = envelope.GreenGhost?.ReleaseData is { } ghostRelease
                ? ToCommonGhostRelease(ghostRelease)
                : null,
            GhostUpdatePerfData = envelope.GreenGhost?.PerfData is { } perf
                ? new CommonPlayResultData.UpdateGhostPerfData
                {
                    InputMedian = perf.InputMedian,
                    InputVariance = perf.InputVariance
                }
                : null,
            GhostUpdateRankData = envelope.GreenGhost?.RankData is { } rank ? ToCommonGhostRank(rank) : null,
            AryStageInfoes = stages.Select(ToCommonStage).ToList()
        };
    }

    private static CommonPlayResultData.StageData ToCommonStage(Ac15StageResult stage)
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
            BattleStageData = stage.BlueBattleStage is { } battle ? ToCommonBattleStage(battle) : null,
            GhostStageData = stage.GreenGhostStage is { } ghost ? ToCommonGhostStage(ghost) : null,
            AryChallengeIds = stage.ChallengeIds.Select(ToCommonCompe).ToList(),
            AryUserCompeIds = stage.UserCompeIds.Select(ToCommonCompe).ToList(),
            AryBngCompeIds = stage.BngCompeIds.Select(ToCommonCompe).ToList(),
            ArySectionDatas = stage.AiSectionData.Select(section => new CommonPlayResultData.AiStageSectionData
            {
                IsWin = section.IsWin,
                Crown = section.Crown,
                Score = section.Score,
                GoodCnt = section.GoodCnt,
                OkCnt = section.OkCnt,
                NgCnt = section.NgCnt,
                PoundCnt = section.PoundCnt
            }).ToList()
        };

    private static CommonPlayResultData.CostumeData ToCommonCostume(Ac15CostumeFacts costume)
        => new()
        {
            Costume1 = costume.Costume1,
            Costume2 = costume.Costume2,
            Costume3 = costume.Costume3,
            Costume4 = costume.Costume4,
            Costume5 = costume.Costume5
        };

    private static CommonPlayResultData.TokkunStageDataDto ToCommonTokkun(Ac15TokkunStageData data)
        => new()
        {
            BanacoinDatetime = data.BanacoinDatetime,
            TokkunSongCnt = data.TokkunSongCnt,
            TookunSongnoes = data.TookunSongnoes,
            TokkunSpeedchangeCnt = data.TokkunSpeedchangeCnt,
            TokkunAutoplayCnt = data.TokkunAutoplayCnt,
            TokkunJumpCnt = data.TokkunJumpCnt
        };

    private static CommonPlayResultData.BattleStageData ToCommonBattleStage(Ac15BlueBattleStageData data)
        => new()
        {
            SupportLv = data.SupportLv,
            BattleStageId = data.BattleStageId,
            NpcData = data.NpcData is { } npc
                ? new CommonPlayResultData.BattleNpcData
                {
                    NpcId = npc.NpcId,
                    AcquiredExp = npc.AcquiredExp,
                    TotalExp = npc.TotalExp,
                    Dpn = npc.Dpn,
                    NpcCostumeId = npc.NpcCostumeId,
                    SpecialId1 = npc.SpecialId1,
                    SpecialId2 = npc.SpecialId2,
                    SpecialId3 = npc.SpecialId3,
                    BondsLv = npc.BondsLv
                }
                : null,
            KillCnt = data.KillCnt,
            BossLife = data.BossLife,
            TotalDamage = data.TotalDamage,
            CriticalCnt = data.CriticalCnt,
            SpecialMoveCnt = data.SpecialMoveCnt
        };

    private static CommonPlayResultData.BattleReleaseDataDto ToCommonBattleRelease(Ac15BlueBattleReleaseData data)
        => new()
        {
            ReleaseInfoIds = data.ReleaseInfoIds,
            ReleaseBattleStageIds = data.ReleaseBattleStageIds,
            ReleaseNpcIds = data.ReleaseNpcIds,
            ReleaseNpcCostumeIds = data.ReleaseNpcCostumeIds,
            ReleaseNpcSpecialIds = data.ReleaseNpcSpecialIds,
            BattleTokenData = data.BattleTokenData.Select(token => new CommonPlayResultData.BattleTokenData
            {
                TokenId = token.TokenId,
                TokenValue = token.TokenValue
            }).ToList(),
            AssignNextStageId = data.AssignNextStageId
        };

    private static CommonPlayResultData.GhostStageData ToCommonGhostStage(Ac15GreenGhostStageData data)
        => new()
        {
            IsWin = data.IsWin,
            SdCertifiedLevelId = data.SdCertifiedLevelId,
            ArySectionData = data.ArySectionData.Select(section => new CommonPlayResultData.GhostStageSectionData
            {
                IsWin = section.IsWin,
                GoodCnt = section.GoodCnt,
                OkCnt = section.OkCnt,
                NgCnt = section.NgCnt,
                PoundCnt = section.PoundCnt
            }).ToList()
        };

    private static CommonPlayResultData.UpdateGhostInfoData ToCommonGhostRelease(Ac15GreenGhostReleaseData data)
        => new()
        {
            ReleaseInfoId = data.ReleaseInfoId,
            AryTokendata = data.AryTokendata.Select(token => new CommonPlayResultData.GhostTokenData
            {
                TokenId = token.TokenId,
                TokenValue = token.TokenValue
            }).ToList()
        };

    private static CommonPlayResultData.UpdateGhostRankData ToCommonGhostRank(Ac15GreenGhostRankData data)
        => new()
        {
            RankId = data.RankId,
            WinPoint = data.WinPoint,
            CertifiedLevelId = data.CertifiedLevelId,
            AryWinningsData = data.AryWinningsData.Select(row => new CommonPlayResultData.GhostWinningsData
            {
                LevelId = row.LevelId,
                Winnings = row.Winnings
            }).ToList()
        };

    private static CommonPlayResultData.ResultcompeData ToCommonCompe(Ac15CompeIdFact fact)
        => new()
        {
            CompeId = fact.CompeId,
            TrackNo = fact.TrackNo
        };
}
