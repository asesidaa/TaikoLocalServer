using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint MinGreenCourseLevel = 1;
    private const uint MaxGreenCourseLevel = 5;
    private const uint MaxGreenStageMode = 1;
    private const uint MaxGreenPlayResult = 3;

    private partial async ValueTask<uint> HandleGreen(
        UpdatePlayResultCommand request,
        CancellationToken cancellationToken)
    {
        var playResultData = request.PlayResultData;
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();
        if (!CanAdd(saveData.TotalGetDonmedal, playResultData.GetDonmedal)
            || !CanAdd(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal)
            || playResultData.AryStageInfoes.Any(stage => !IsValidGreenStage(stage, green))
            || !HasOnlyInRangeUnlockRewards(playResultData)
            || (playResultData.HasAryCurrentCostume && !IsValidCurrentCostume(saveData, playResultData.AryCurrentCostume)))
        {
            logger.LogWarning("Rejecting invalid Green playresult payload for baid {Baid}", request.Baid);
            return 0;
        }

        var playTime = DateTime.TryParse(playResultData.PlayDatetime, out var parsed)
            ? parsed
            : DateTime.Now;

        saveData.TotalGetDonmedal += playResultData.GetDonmedal;
        saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;
        saveData.ItemshopTutorialFlg = playResultData.ItemshopTutorialFlg ?? saveData.ItemshopTutorialFlg;
        saveData.IsDevil = playResultData.IsDevil ?? saveData.IsDevil;
        saveData.IsExplain = playResultData.IsExplain ?? saveData.IsExplain;
        saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;
        if (playResultData.HasDifficultyPlayedCourse)
        {
            saveData.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
        }

        if (playResultData.HasDifficultyPlayedStar)
        {
            saveData.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
        }

        saveData.LastPlayDatetime = playTime;
        saveData.PrevAreaCode = playResultData.AreaCode;

        if (playResultData.HasAryCurrentCostume)
        {
            ApplyCostume(saveData, playResultData.AryCurrentCostume);
        }

        ApplyUnlockBits(saveData, playResultData);
        ApplyGhostUpdates(saveData, playResultData);

        foreach (var stage in playResultData.AryStageInfoes)
        {
            GreenProfileCounters.ApplyStage(saveData, stage);
            await SaveStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
        }

        await SaveGreenDanAsync(saveData, playResultData, green, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return 1;
    }

    private static bool IsValidGreenStage(CommonPlayResultData.StageData stage, IGreenCatalog green)
    {
        return stage.SongNo < GreenProtocolBytes.SongFlagBytes * 8
            && green.GreenMusicInfos.ContainsKey(stage.SongNo)
            && stage.Level is >= MinGreenCourseLevel and <= MaxGreenCourseLevel
            && stage.StageMode <= MaxGreenStageMode
            && stage.PlayResult <= MaxGreenPlayResult
            && stage.MusicCateg <= 7
            && (stage.PlayDan is null || GreenDanHelpers.IsKnownGreenDanId(stage.PlayDan.Value));
    }

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static bool HasBit(byte[] source, uint id, int byteCount)
    {
        if (id >= byteCount * 8)
        {
            return false;
        }

        var fixedBytes = GreenProtocolBytes.FixedOrZero(source, byteCount);
        return (fixedBytes[id >> 3] & (1 << ((int)id & 7))) != 0;
    }

    private static bool IsValidCurrentCostume(UserSaveDataGreen saveData, CommonPlayResultData.CostumeData costume)
    {
        return HasBit(saveData.CostumeFlg1, costume.Costume1, GreenProtocolBytes.CostumeFlagBytes)
            && HasBit(saveData.CostumeFlg2, costume.Costume2, GreenProtocolBytes.CostumeFlagBytes)
            && HasBit(saveData.CostumeFlg3, costume.Costume3, GreenProtocolBytes.CostumeFlagBytes)
            && HasBit(saveData.CostumeFlg4, costume.Costume4, GreenProtocolBytes.CostumeFlagBytes)
            && HasBit(saveData.CostumeFlg5, costume.Costume5, GreenProtocolBytes.CostumeFlagBytes);
    }

    private static bool HasOnlyInRangeUnlockRewards(CommonPlayResultData playResultData)
    {
        return AllWithinRange(playResultData.GetToneNoes, GreenProtocolBytes.ToneFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes);
    }

    private static bool AllWithinRange(IEnumerable<uint> ids, int byteCount)
    {
        var maxBits = (uint)(byteCount * 8);
        return ids.All(id => id < maxBits);
    }

    private static void ApplyCostume(UserSaveDataGreen saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
    }

    private static void ApplyUnlockBits(UserSaveDataGreen saveData, CommonPlayResultData playResultData)
    {
        saveData.ToneFlg = SetBits(saveData.ToneFlg, playResultData.GetToneNoes, GreenProtocolBytes.ToneFlagBytes);
        saveData.CostumeFlg1 = SetBits(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBits(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBits(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBits(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBits(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.TitleFlg = SetBits(saveData.TitleFlg, playResultData.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes);
    }

    private async Task SaveStageAsync(
        uint baid,
        CommonPlayResultData.StageData stage,
        uint playMode,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        var difficulty = GreenPlayResultMapping.MapDifficulty(stage.Level);
        var crown = GreenPlayResultMapping.MapCrown(stage.PlayResult);
        var isShin = stage.StageMode == 1;
        var play = new SongPlayDatumGreen
        {
            Baid = baid,
            SongId = stage.SongNo,
            Difficulty = difficulty,
            Crown = crown,
            Score = stage.PlayScore,
            ScoreRate = stage.ScoreRate,
            GoodCount = stage.GoodCnt,
            OkCount = stage.OkCnt,
            MissCount = stage.NgCnt,
            ComboCount = stage.ComboCnt,
            HitCount = stage.HitCnt,
            PoundCount = stage.PoundCnt,
            StarLevel = stage.StarLevel,
            SupportLevel = stage.SupportLevel,
            OptionFlg = stage.OptionFlg,
            ToneFlg = stage.ToneFlg,
            PlayMode = playMode,
            StageMode = stage.StageMode,
            IsShin = isShin,
            MusicCategory = stage.MusicCateg,
            SelectedFolderId = stage.SelectedFolderId,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            IsPapamama = stage.IsPapamama,
            IsPushed = stage.IsPushed,
            SoulGauge = stage.SoulGauge.GetValueOrDefault(),
            PlayDan = stage.PlayDan.GetValueOrDefault(),
            WaiwaiResult = stage.WaiwaiResult.GetValueOrDefault(),
            WaiwaiGauge = stage.WaiwaiGauge.GetValueOrDefault(),
            PlayTime = playTime
        };

        context.SongPlayDataGreen.Add(play);

        if (stage.GhostStageData is not null)
        {
            uint sectionNo = 0;
            foreach (var section in stage.GhostStageData.ArySectionData)
            {
                context.GhostStageSectionDataGreen.Add(new GhostStageSectionDatumGreen
                {
                    Parent = play,
                    SectionNo = sectionNo++,
                    IsWin = section.IsWin,
                    GoodCount = section.GoodCnt,
                    OkCount = section.OkCnt,
                    NgCount = section.NgCnt,
                    PoundCount = section.PoundCnt
                });
            }
        }

        await UpsertBestAsync(baid, stage, difficulty, crown, isShin, cancellationToken);
        await UpsertFavoriteAndRecentAsync(baid, stage, playTime, cancellationToken);
    }

    private async Task UpsertBestAsync(
        uint baid,
        CommonPlayResultData.StageData stage,
        Difficulty difficulty,
        CrownType crown,
        bool isShin,
        CancellationToken cancellationToken)
    {
        var existing = await context.SongBestDataGreen.FindAsync([baid, stage.SongNo, difficulty, isShin], cancellationToken);
        if (existing is null)
        {
            context.SongBestDataGreen.Add(new SongBestDatumGreen
            {
                Baid = baid,
                SongId = stage.SongNo,
                Difficulty = difficulty,
                IsShin = isShin,
                BestScore = stage.PlayScore,
                BestRate = stage.ScoreRate,
                BestCrown = crown
            });
            return;
        }

        if (stage.PlayScore > existing.BestScore)
        {
            existing.BestScore = stage.PlayScore;
            existing.BestRate = stage.ScoreRate;
        }

        if (CrownRank(crown) > CrownRank(existing.BestCrown))
        {
            existing.BestCrown = crown;
        }
    }

    private async Task UpsertFavoriteAndRecentAsync(
        uint baid,
        CommonPlayResultData.StageData stage,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        var favorite = await context.GreenFavoriteSongs.FindAsync([baid, stage.SongNo], cancellationToken);
        if (stage.IsFavorite && favorite is null)
        {
            var count = await context.GreenFavoriteSongs.CountAsync(s => s.Baid == baid, cancellationToken);
            if (count < 5)
            {
                context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = baid, SongNo = stage.SongNo });
            }
        }
        else if (!stage.IsFavorite && favorite is not null)
        {
            context.GreenFavoriteSongs.Remove(favorite);
        }

        var recent = await context.GreenRecentSongs.FindAsync([baid, stage.SongNo], cancellationToken);
        if (recent is null)
        {
            context.GreenRecentSongs.Add(new GreenRecentSongs
            {
                Baid = baid,
                SongNo = stage.SongNo,
                LastPlayed = playTime
            });
        }
        else
        {
            recent.LastPlayed = playTime;
        }

        await context.SaveChangesAsync(cancellationToken);

        var overage = await context.GreenRecentSongs
            .Where(s => s.Baid == baid)
            .OrderByDescending(s => s.LastPlayed)
            .Skip(10)
            .ToListAsync(cancellationToken);
        if (overage.Count > 0)
        {
            context.GreenRecentSongs.RemoveRange(overage);
        }
    }

    private async Task SaveGreenDanAsync(
        UserSaveDataGreen saveData,
        CommonPlayResultData playResultData,
        IGreenCatalog green,
        CancellationToken cancellationToken)
    {
        if (playResultData.PlayMode != 1)
        {
            return;
        }

        var danIds = playResultData.AryStageInfoes
            .Select(stage => stage.PlayDan.GetValueOrDefault())
            .Where(dan => dan != 0)
            .Distinct()
            .ToArray();

        if (danIds.Length != 1)
        {
            logger.LogWarning("Skipping Green Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveData.Baid, danIds.Length);
            return;
        }

        if (playResultData.DanResult > (uint)GreenDanClearGrade.GoldClear)
        {
            logger.LogWarning("Skipping Green Dani save for baid {Baid}: invalid DanResult {DanResult}", saveData.Baid, playResultData.DanResult);
            return;
        }

        var danId = danIds[0];
        var pack = green.TaikojukuFileOrder.FirstOrDefault(row => row.ChallengeLevel == danId);
        if (pack is null || !GreenDanHelpers.IsKnownGreenDanId(danId))
        {
            logger.LogWarning("Skipping Green Dani save for baid {Baid}: unknown Dan id {DanId}", saveData.Baid, danId);
            return;
        }

        var isExtra = GreenDanHelpers.IsExtraDanId(danId);
        var danScore = await context.DanScoreDataGreen
            .Include(row => row.DanStageScoreData)
            .SingleOrDefaultAsync(row => row.Baid == saveData.Baid && row.DanId == danId && row.IsExtra == isExtra, cancellationToken);

        if (danScore is null)
        {
            danScore = new DanScoreDatumGreen
            {
                Baid = saveData.Baid,
                DanId = danId,
                IsExtra = isExtra,
                MedleyUniqueId = pack.UniqueId
            };
            context.DanScoreDataGreen.Add(danScore);
        }

        UpdateGreenDanScore(danScore, playResultData);
        await UpdateGreenDanSummaryAsync(saveData, danScore, cancellationToken);
    }

    private static void UpdateGreenDanScore(DanScoreDatumGreen danScore, CommonPlayResultData playResultData)
    {
        danScore.ClearGrade = GreenDanHelpers.ClampGrade(Math.Max((uint)danScore.ClearGrade, playResultData.DanResult));
        danScore.ArrivalSongCount = Math.Max(danScore.ArrivalSongCount, (uint)playResultData.AryStageInfoes.Count);
        danScore.ComboCountTotal = Math.Max(danScore.ComboCountTotal, playResultData.ComboCntTotal);
        danScore.SoulGaugeTotal = Math.Max(danScore.SoulGaugeTotal, playResultData.SoulGaugeTotal);

        for (var i = 0; i < playResultData.AryStageInfoes.Count; i++)
        {
            var stage = playResultData.AryStageInfoes[i];
            var stageIndex = (uint)i;
            var existing = danScore.DanStageScoreData.FirstOrDefault(row => row.StageIndex == stageIndex);
            if (existing is null)
            {
                existing = new DanStageScoreDatumGreen
                {
                    Baid = danScore.Baid,
                    DanId = danScore.DanId,
                    IsExtra = danScore.IsExtra,
                    StageIndex = stageIndex,
                    SongNumber = stage.SongNo,
                    BadCount = stage.NgCnt
                };
                danScore.DanStageScoreData.Add(existing);
            }

            existing.SongNumber = stage.SongNo;
            existing.PlayScore = Math.Max(existing.PlayScore, stage.PlayScore);
            existing.HighScore = Math.Max(existing.HighScore, stage.PlayScore);
            existing.ComboCount = Math.Max(existing.ComboCount, stage.ComboCnt);
            existing.DrumrollCount = Math.Max(existing.DrumrollCount, stage.PoundCnt);
            existing.GoodCount = Math.Max(existing.GoodCount, stage.GoodCnt);
            existing.OkCount = Math.Max(existing.OkCount, stage.OkCnt);
            existing.TotalHitCount = Math.Max(existing.TotalHitCount, stage.HitCnt);
            existing.BadCount = Math.Min(existing.BadCount, stage.NgCnt);
        }
    }

    private async ValueTask UpdateGreenDanSummaryAsync(
        UserSaveDataGreen saveData,
        DanScoreDatumGreen currentDanScore,
        CancellationToken cancellationToken)
    {
        var rows = await context.DanScoreDataGreen
            .Where(row => row.Baid == saveData.Baid)
            .ToListAsync(cancellationToken);

        if (!rows.Any(row => row.DanId == currentDanScore.DanId && row.IsExtra == currentDanScore.IsExtra))
        {
            rows.Add(currentDanScore);
        }

        var normalGrades = rows
            .Where(row => !row.IsExtra && GreenDanHelpers.IsNormalDanId(row.DanId))
            .ToDictionary(row => row.DanId, row => row.ClearGrade);

        var normalFlags = new byte[GreenProtocolBytes.DanFlagBytes];
        foreach (var row in rows.Where(row => !row.IsExtra && GreenDanHelpers.IsNormalDanId(row.DanId)))
        {
            normalFlags = GreenDanHelpers.SetPackedGrade(
                normalFlags,
                GreenDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        var extraFlags = new byte[GreenProtocolBytes.DanExtraFlagBytes];
        foreach (var row in rows.Where(row => row.IsExtra && GreenDanHelpers.IsExtraDanId(row.DanId)))
        {
            extraFlags = GreenDanHelpers.SetPackedGrade(
                extraFlags,
                GreenDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        saveData.GotDanFlg = normalFlags;
        saveData.GotDanExtraFlg = extraFlags;
        saveData.GotDanMax = GreenDanHelpers.GetGotDanMax(normalGrades);
        saveData.DispTaikojukuDan = GreenDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalGrades);
    }

    private void ApplyGhostUpdates(UserSaveDataGreen saveData, CommonPlayResultData playResultData)
    {
        if (playResultData.GhostReleaseData is not null)
        {
            saveData.GhostReleaseInfoFlag = SetBits(
                saveData.GhostReleaseInfoFlag,
                playResultData.GhostReleaseData.ReleaseInfoId,
                GreenProtocolBytes.GhostReleaseInfoBytes);

            foreach (var token in playResultData.GhostReleaseData.AryTokendata)
            {
                var existing = context.GreenGhostTokens.Find(saveData.Baid, token.TokenId);
                if (existing is null)
                {
                    context.GreenGhostTokens.Add(new GreenGhostTokens
                    {
                        Baid = saveData.Baid,
                        TokenId = token.TokenId,
                        TokenValue = token.TokenValue
                    });
                }
                else
                {
                    existing.TokenValue = token.TokenValue;
                }
            }
        }

        if (playResultData.GhostUpdatePerfData is not null)
        {
            saveData.GhostInputMedian = playResultData.GhostUpdatePerfData.InputMedian;
            saveData.GhostInputVariance = playResultData.GhostUpdatePerfData.InputVariance;
        }

        if (playResultData.GhostUpdateRankData is null)
        {
            return;
        }

        saveData.GhostRankId = playResultData.GhostUpdateRankData.RankId;
        saveData.GhostWinPoint = playResultData.GhostUpdateRankData.WinPoint;
        saveData.GhostCertifiedLevelId = playResultData.GhostUpdateRankData.CertifiedLevelId;
        saveData.GhostTotalWinnings = (uint)Math.Min(
            uint.MaxValue,
            playResultData.GhostUpdateRankData.AryWinningsData.Sum(row => (long)row.Winnings));

        foreach (var winning in playResultData.GhostUpdateRankData.AryWinningsData)
        {
            var existing = context.GreenGhostWinnings.Find(saveData.Baid, winning.LevelId);
            if (existing is null)
            {
                context.GreenGhostWinnings.Add(new GreenGhostWinnings
                {
                    Baid = saveData.Baid,
                    LevelId = winning.LevelId,
                    Winnings = winning.Winnings
                });
            }
            else
            {
                existing.Winnings = winning.Winnings;
            }
        }
    }

    private static byte[] SetBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = GreenProtocolBytes.FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}
