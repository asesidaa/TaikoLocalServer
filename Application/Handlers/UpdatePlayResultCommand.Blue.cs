using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint MinBlueCourseLevel = 1;
    private const uint MaxBlueCourseLevel = 5;
    private const int BlueMaxRecentSongs = 10;
    private const int BlueMaxFavoriteSongs = 5;

    private partial async ValueTask<uint> HandleBlue(
        UpdatePlayResultCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Baid == 0)
        {
            return 1;
        }

        var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Game uploading a non existing Blue user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var blue = gameDataService.Blue();

        if (!CanAddBlue(saveData.TotalGetDonmedal, playResultData.GetDonmedal)
            || !CanAddBlue(saveData.TotalGetKatsumedal, playResultData.GetKatsumedal))
        {
            logger.LogWarning("Rejecting invalid Blue medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        var playTime = DateTime.TryParse(playResultData.PlayDatetime, out var parsed)
            ? parsed
            : DateTime.Now;

        if (!DateTime.TryParse(playResultData.PlayDatetime, out _))
        {
            logger.LogWarning(
                "Blue playresult for baid {Baid} had invalid play_datetime {PlayDatetime}; using server time",
                request.Baid,
                playResultData.PlayDatetime);
        }

        if (playResultData.HasBattleStageData || playResultData.HasReleaseBattleData || playResultData.HasTokkunStageInfo)
        {
            logger.LogWarning(
                "Blue playresult for baid {Baid} contained deferred fields: battle_stage={BattleStage} release_battle={ReleaseBattle} tokkun={Tokkun}",
                request.Baid,
                playResultData.HasBattleStageData,
                playResultData.HasReleaseBattleData,
                playResultData.HasTokkunStageInfo);
        }

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

        if (playResultData.HasAryCurrentCostume && saveData.IsAutoCostumeOn)
        {
            ApplyCostume(saveData, playResultData.AryCurrentCostume);
        }

        ApplyUnlockBits(saveData, playResultData);

        foreach (var stage in playResultData.AryStageInfoes)
        {
            if (!IsSupportedBlueStage(request.Baid, stage))
            {
                continue;
            }

            BlueProfileCounters.ApplyStage(saveData, stage);
            await SaveBlueStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
        }

        await SaveBlueDanAsync(saveData, playResultData, blue, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        await TrimBlueRecentSongsAsync(request.Baid, cancellationToken);
        return 1;
    }

    private bool IsSupportedBlueStage(uint baid, CommonPlayResultData.StageData stage)
    {
        if (!BluePlayResultMapping.IsSupportedNormalStageMode(stage.StageMode))
        {
            logger.LogWarning(
                "Skipping unsupported Blue stage mode for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        if (stage.SongNo >= BlueProtocolBytes.SongFlagBytes * 8 || stage.Level is < MinBlueCourseLevel or > MaxBlueCourseLevel)
        {
            logger.LogWarning(
                "Skipping invalid Blue stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                baid,
                stage.SongNo,
                stage.Level,
                stage.StageMode);
            return false;
        }

        return true;
    }

    private static void ApplyCostume(UserSaveDataBlue saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, [costume.Costume1], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBlueBits(saveData.CostumeFlg2, [costume.Costume2], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBlueBits(saveData.CostumeFlg3, [costume.Costume3], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBlueBits(saveData.CostumeFlg4, [costume.Costume4], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBlueBits(saveData.CostumeFlg5, [costume.Costume5], BlueProtocolBytes.CostumeFlagBytes);
    }

    private static void ApplyUnlockBits(UserSaveDataBlue saveData, CommonPlayResultData playResultData)
    {
        saveData.ReleaseSongFlg = SetBlueBits(saveData.ReleaseSongFlg, playResultData.ReleaseSongNoes, BlueProtocolBytes.SongFlagBytes);
        saveData.ToneFlg = SetBlueBits(saveData.ToneFlg, playResultData.GetToneNoes, BlueProtocolBytes.ToneFlagBytes);
        saveData.CostumeFlg1 = SetBlueBits(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBlueBits(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBlueBits(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBlueBits(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBlueBits(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, BlueProtocolBytes.CostumeFlagBytes);
        saveData.TitleFlg = SetBlueBits(saveData.TitleFlg, playResultData.GetTitleNoes, BlueProtocolBytes.TitleFlagBytes);
    }

    private async Task SaveBlueStageAsync(
        uint baid,
        CommonPlayResultData.StageData stage,
        uint playMode,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        var difficulty = BluePlayResultMapping.MapDifficulty(stage.Level);
        var crown = BluePlayResultMapping.MapCrown(stage.PlayResult);
        var isShin = BluePlayResultMapping.IsShin(stage.StageMode);

        context.SongPlayDataBlue.Add(new SongPlayDatumBlue
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
        });

        await UpsertBestAsync(baid, stage, difficulty, crown, isShin, cancellationToken);
        await UpsertBlueFavoriteAndRecentAsync(baid, stage, playTime, cancellationToken);
    }

    private async Task UpsertBestAsync(
        uint baid,
        CommonPlayResultData.StageData stage,
        Difficulty difficulty,
        CrownType crown,
        bool isShin,
        CancellationToken cancellationToken)
    {
        var existing = await context.SongBestDataBlue.FindAsync([baid, stage.SongNo, difficulty, isShin], cancellationToken);
        if (existing is null)
        {
            context.SongBestDataBlue.Add(new SongBestDatumBlue
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

        if (BluePlayResultMapping.CrownRank(crown) > BluePlayResultMapping.CrownRank(existing.BestCrown))
        {
            existing.BestCrown = crown;
        }
    }

    private async Task UpsertBlueFavoriteAndRecentAsync(
        uint baid,
        CommonPlayResultData.StageData stage,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        var favorite = await context.BlueFavoriteSongs.FindAsync([baid, stage.SongNo], cancellationToken);
        if (stage.IsFavorite && favorite is null)
        {
            var persistedFavoriteSongNoes = await context.BlueFavoriteSongs
                .Where(s => s.Baid == baid)
                .Select(s => s.SongNo)
                .ToArrayAsync(cancellationToken);
            var trackedFavoriteSongNoes = context.BlueFavoriteSongs.Local
                .Where(s => s.Baid == baid)
                .Select(s => s.SongNo);
            var favoriteCount = persistedFavoriteSongNoes
                .Concat(trackedFavoriteSongNoes)
                .Distinct()
                .Count();

            if (favoriteCount < BlueMaxFavoriteSongs)
            {
                context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = baid, SongNo = stage.SongNo });
            }
        }
        else if (!stage.IsFavorite && favorite is not null)
        {
            context.BlueFavoriteSongs.Remove(favorite);
        }

        var recent = await context.BlueRecentSongs.FindAsync([baid, stage.SongNo], cancellationToken);
        if (recent is null)
        {
            context.BlueRecentSongs.Add(new BlueRecentSongs
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
    }

    private async Task TrimBlueRecentSongsAsync(uint baid, CancellationToken cancellationToken)
    {
        var overage = await context.BlueRecentSongs
            .Where(s => s.Baid == baid)
            .OrderByDescending(s => s.LastPlayed)
            .Skip(BlueMaxRecentSongs)
            .ToListAsync(cancellationToken);
        if (overage.Count == 0)
        {
            return;
        }

        context.BlueRecentSongs.RemoveRange(overage);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveBlueDanAsync(
        UserSaveDataBlue saveData,
        CommonPlayResultData playResultData,
        IBlueCatalog blue,
        CancellationToken cancellationToken)
    {
        if (playResultData.PlayMode != (uint)PlayMode.DanMode)
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
            logger.LogWarning("Skipping Blue Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveData.Baid, danIds.Length);
            return;
        }

        if (playResultData.DanResult > (uint)BlueDanClearGrade.GoldClear)
        {
            logger.LogWarning("Skipping Blue Dani save for baid {Baid}: invalid DanResult {DanResult}", saveData.Baid, playResultData.DanResult);
            return;
        }

        var danId = danIds[0];
        var pack = blue.TaikojukuFileOrder.FirstOrDefault(row => row.ChallengeLevel == danId);
        if (pack is null || !BlueDanHelpers.IsKnownBlueDanId(danId))
        {
            logger.LogWarning("Skipping Blue Dani save for baid {Baid}: unknown Dan id {DanId}", saveData.Baid, danId);
            return;
        }

        var isExtra = BlueDanHelpers.IsExtraDanId(danId);
        var danScore = await context.DanScoreDataBlue
            .Include(row => row.DanStageScoreData)
            .SingleOrDefaultAsync(row => row.Baid == saveData.Baid && row.DanId == danId && row.IsExtra == isExtra, cancellationToken);

        if (danScore is null)
        {
            danScore = new DanScoreDatumBlue
            {
                Baid = saveData.Baid,
                DanId = danId,
                IsExtra = isExtra,
                MedleyUniqueId = pack.UniqueId
            };
            context.DanScoreDataBlue.Add(danScore);
        }

        var incomingClearGrade = BlueDanHelpers.ClampGrade(playResultData.DanResult);
        UpdateBlueDanScore(danScore, playResultData);
        await UpdateBlueDanSummaryAsync(saveData, danScore, incomingClearGrade, cancellationToken);
    }

    private static void UpdateBlueDanScore(DanScoreDatumBlue danScore, CommonPlayResultData playResultData)
    {
        danScore.ClearGrade = BlueDanHelpers.ClampGrade(Math.Max((uint)danScore.ClearGrade, playResultData.DanResult));
        danScore.ArrivalSongCount = Math.Max(danScore.ArrivalSongCount, (uint)playResultData.AryStageInfoes.Count);
        danScore.ComboCountTotal = Math.Max(danScore.ComboCountTotal, playResultData.ComboCntTotal);
        danScore.SoulGaugeTotal = Math.Max(
            danScore.SoulGaugeTotal,
            playResultData.AryStageInfoes.LastOrDefault()?.SoulGauge.GetValueOrDefault() ?? 0);

        for (var i = 0; i < playResultData.AryStageInfoes.Count; i++)
        {
            var stage = playResultData.AryStageInfoes[i];
            var stageIndex = (uint)i;
            var existing = danScore.DanStageScoreData.FirstOrDefault(row => row.StageIndex == stageIndex);
            if (existing is null)
            {
                existing = new DanStageScoreDatumBlue
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

    private async ValueTask UpdateBlueDanSummaryAsync(
        UserSaveDataBlue saveData,
        DanScoreDatumBlue currentDanScore,
        BlueDanClearGrade incomingClearGrade,
        CancellationToken cancellationToken)
    {
        var rows = await context.DanScoreDataBlue
            .Where(row => row.Baid == saveData.Baid)
            .ToListAsync(cancellationToken);

        if (!rows.Any(row => row.DanId == currentDanScore.DanId && row.IsExtra == currentDanScore.IsExtra))
        {
            rows.Add(currentDanScore);
        }

        var normalGrades = rows
            .Where(row => !row.IsExtra && BlueDanHelpers.IsNormalDanId(row.DanId))
            .ToDictionary(row => row.DanId, row => row.ClearGrade);

        var normalFlags = new byte[BlueProtocolBytes.DanFlagBytes];
        foreach (var row in rows.Where(row => !row.IsExtra && BlueDanHelpers.IsNormalDanId(row.DanId)))
        {
            normalFlags = BlueDanHelpers.SetPackedGrade(
                normalFlags,
                BlueDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        var extraFlags = new byte[BlueProtocolBytes.DanExtraFlagBytes];
        foreach (var row in rows.Where(row => row.IsExtra && BlueDanHelpers.IsExtraDanId(row.DanId)))
        {
            extraFlags = BlueDanHelpers.SetPackedGrade(
                extraFlags,
                BlueDanHelpers.GetPackedIndex(row.DanId),
                row.ClearGrade);
        }

        var isIncomingClear = BlueDanHelpers.IsClear(incomingClearGrade);

        saveData.GotDanFlg = normalFlags;
        saveData.GotDanExtraFlg = extraFlags;
        saveData.GotDanMax = BlueDanHelpers.GetGotDanMax(normalGrades);
        saveData.DispTaikojukuDan = !currentDanScore.IsExtra
                                    && BlueDanHelpers.IsNormalDanId(currentDanScore.DanId)
                                    && isIncomingClear
            ? BlueDanHelpers.GetDisplayDanAfterNormalClear(currentDanScore.DanId)
            : BlueDanHelpers.NormalizeDisplayDan(saveData.DispTaikojukuDan, normalGrades);
    }

    private static bool CanAddBlue(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static byte[] SetBlueBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = BlueProtocolBytes.FixedOrZero(source, byteCount);
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
}
