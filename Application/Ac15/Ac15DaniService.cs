namespace TaikoLocalServer.Application.Ac15;

public static class Ac15DaniService
{
    public static async ValueTask SaveAsync(
        ITaikoDbContext context,
        CommonPlayResultData playResultData,
        Ac15EraProfile profile,
        IEnumerable<Ac15DaniChallenge> challenges,
        Ac15DaniSaveState saveState,
        Action<Ac15DaniSaveUpdate> applySaveUpdate,
        ILogger logger,
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
            logger.LogWarning(
                "Skipping {Era} Dani save for baid {Baid}: expected one PlayDan value, got {Count}",
                profile.Era,
                saveState.Baid,
                danIds.Length);
            return;
        }

        if (playResultData.DanResult > (uint)Ac15DanClearGrade.GoldClear)
        {
            logger.LogWarning(
                "Skipping {Era} Dani save for baid {Baid}: invalid DanResult {DanResult}",
                profile.Era,
                saveState.Baid,
                playResultData.DanResult);
            return;
        }

        var danId = danIds[0];
        var limits = profile.Limits;
        var knownChallengeLevels = challenges.Select(row => row.DanId).ToHashSet();
        var challenge = challenges.FirstOrDefault(row => row.DanId == danId);
        if (challenge is null
            || !Ac15DanHelpers.IsKnownDanId(danId, limits)
            || !knownChallengeLevels.Contains(danId))
        {
            logger.LogWarning(
                "Skipping {Era} Dani save for baid {Baid}: unknown Dan id {DanId}",
                profile.Era,
                saveState.Baid,
                danId);
            return;
        }

        var isExtra = Ac15DanHelpers.IsExtraDanId(danId, limits);
        var key = new Ac15DaniScoreKey(saveState.Baid, danId, isExtra);
        var existing = await GetScoreAsync(context, profile, key, cancellationToken);
        var updatedScore = BuildUpdatedScore(saveState.Baid, existing, danId, isExtra, challenge.MedleyUniqueId, playResultData);
        var summaries = (await GetScoreSummariesAsync(context, profile, saveState.Baid, cancellationToken)).ToList();

        await UpsertScoreAsync(context, profile, updatedScore, cancellationToken);

        summaries.RemoveAll(row => row.DanId == updatedScore.DanId && row.IsExtra == updatedScore.IsExtra);
        summaries.Add(new Ac15DaniScoreSummary(updatedScore.DanId, updatedScore.IsExtra, updatedScore.ClearGrade));

        applySaveUpdate(BuildSaveUpdate(updatedScore, summaries, saveState, limits, Ac15DanHelpers.ClampGrade(playResultData.DanResult)));
    }

    public static async ValueTask<IReadOnlyList<Ac15DaniScore>> GetScoresAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        IReadOnlySet<uint> requestedDanIds,
        IReadOnlySet<uint> knownChallengeLevels,
        CancellationToken cancellationToken)
    {
        var validRequestedIds = requestedDanIds
            .Where(knownChallengeLevels.Contains)
            .ToHashSet();

        return profile.Era switch
        {
            GameEra.Blue => (await context.DanScoreDataBlue
                    .Where(row => row.Baid == baid && validRequestedIds.Contains(row.DanId))
                    .Include(row => row.DanStageScoreData)
                    .ToListAsync(cancellationToken))
                .OrderBy(row => row.DanId)
                .Select(Ac15DaniMapper.ToAc15DaniScore)
                .ToArray(),
            GameEra.Green => (await context.DanScoreDataGreen
                    .Where(row => row.Baid == baid && validRequestedIds.Contains(row.DanId))
                    .Include(row => row.DanStageScoreData)
                    .ToListAsync(cancellationToken))
                .OrderBy(row => row.DanId)
                .Select(Ac15DaniMapper.ToAc15DaniScore)
                .ToArray(),
            GameEra.Yellow => (await context.DanScoreDataYellow
                    .Where(row => row.Baid == baid && validRequestedIds.Contains(row.DanId))
                    .Include(row => row.DanStageScoreData)
                    .ToListAsync(cancellationToken))
                .OrderBy(row => row.DanId)
                .Select(Ac15DaniMapper.ToAc15DaniScore)
                .ToArray(),
            _ => []
        };
    }

    private static async ValueTask<Ac15DaniScore?> GetScoreAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        Ac15DaniScoreKey key,
        CancellationToken cancellationToken)
        => profile.Era switch
        {
            GameEra.Blue => await context.DanScoreDataBlue
                .Include(score => score.DanStageScoreData)
                .SingleOrDefaultAsync(
                    score => score.Baid == key.Baid
                             && score.DanId == key.DanId
                             && score.IsExtra == key.IsExtra,
                    cancellationToken) is { } row
                ? Ac15DaniMapper.ToAc15DaniScore(row)
                : null,
            GameEra.Green => await context.DanScoreDataGreen
                .Include(score => score.DanStageScoreData)
                .SingleOrDefaultAsync(
                    score => score.Baid == key.Baid
                             && score.DanId == key.DanId
                             && score.IsExtra == key.IsExtra,
                    cancellationToken) is { } row
                ? Ac15DaniMapper.ToAc15DaniScore(row)
                : null,
            GameEra.Yellow => await context.DanScoreDataYellow
                .Include(score => score.DanStageScoreData)
                .SingleOrDefaultAsync(
                    score => score.Baid == key.Baid
                             && score.DanId == key.DanId
                             && score.IsExtra == key.IsExtra,
                    cancellationToken) is { } row
                ? Ac15DaniMapper.ToAc15DaniScore(row)
                : null,
            _ => null
        };

    private static async ValueTask<IReadOnlyList<Ac15DaniScoreSummary>> GetScoreSummariesAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        CancellationToken cancellationToken)
        => profile.Era switch
        {
            GameEra.Blue => (await context.DanScoreDataBlue
                    .Where(row => row.Baid == baid)
                    .ToListAsync(cancellationToken))
                .Select(Ac15DaniMapper.ToAc15DaniScoreSummary)
                .ToArray(),
            GameEra.Green => (await context.DanScoreDataGreen
                    .Where(row => row.Baid == baid)
                    .ToListAsync(cancellationToken))
                .Select(Ac15DaniMapper.ToAc15DaniScoreSummary)
                .ToArray(),
            GameEra.Yellow => (await context.DanScoreDataYellow
                    .Where(row => row.Baid == baid)
                    .ToListAsync(cancellationToken))
                .Select(Ac15DaniMapper.ToAc15DaniScoreSummary)
                .ToArray(),
            _ => []
        };

    private static async ValueTask UpsertScoreAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        Ac15DaniScore score,
        CancellationToken cancellationToken)
    {
        switch (profile.Era)
        {
            case GameEra.Blue:
                await UpsertBlueScoreAsync(context, score, cancellationToken);
                return;
            case GameEra.Green:
                await UpsertGreenScoreAsync(context, score, cancellationToken);
                return;
            case GameEra.Yellow:
                await UpsertYellowScoreAsync(context, score, cancellationToken);
                return;
        }
    }

    private static async ValueTask UpsertBlueScoreAsync(
        ITaikoDbContext context,
        Ac15DaniScore score,
        CancellationToken cancellationToken)
    {
        var row = await context.DanScoreDataBlue
            .Include(existing => existing.DanStageScoreData)
            .SingleOrDefaultAsync(
                existing => existing.Baid == score.Baid
                            && existing.DanId == score.DanId
                            && existing.IsExtra == score.IsExtra,
                cancellationToken);

        if (row is null)
        {
            row = Ac15DaniMapper.ToBlueDanScoreDatum(score);
            UpsertBlueStages(row, score);
            context.DanScoreDataBlue.Add(row);
            return;
        }

        Ac15DaniMapper.ApplyToBlueDanScoreDatum(score, row);
        UpsertBlueStages(row, score);
    }

    private static async ValueTask UpsertGreenScoreAsync(
        ITaikoDbContext context,
        Ac15DaniScore score,
        CancellationToken cancellationToken)
    {
        var row = await context.DanScoreDataGreen
            .Include(existing => existing.DanStageScoreData)
            .SingleOrDefaultAsync(
                existing => existing.Baid == score.Baid
                            && existing.DanId == score.DanId
                            && existing.IsExtra == score.IsExtra,
                cancellationToken);

        if (row is null)
        {
            row = Ac15DaniMapper.ToGreenDanScoreDatum(score);
            UpsertGreenStages(row, score);
            context.DanScoreDataGreen.Add(row);
            return;
        }

        Ac15DaniMapper.ApplyToGreenDanScoreDatum(score, row);
        UpsertGreenStages(row, score);
    }

    private static async ValueTask UpsertYellowScoreAsync(
        ITaikoDbContext context,
        Ac15DaniScore score,
        CancellationToken cancellationToken)
    {
        var row = await context.DanScoreDataYellow
            .Include(existing => existing.DanStageScoreData)
            .SingleOrDefaultAsync(
                existing => existing.Baid == score.Baid
                            && existing.DanId == score.DanId
                            && existing.IsExtra == score.IsExtra,
                cancellationToken);

        if (row is null)
        {
            row = Ac15DaniMapper.ToYellowDanScoreDatum(score);
            UpsertYellowStages(row, score);
            context.DanScoreDataYellow.Add(row);
            return;
        }

        Ac15DaniMapper.ApplyToYellowDanScoreDatum(score, row);
        UpsertYellowStages(row, score);
    }

    private static void UpsertBlueStages(DanScoreDatumBlue row, Ac15DaniScore score)
    {
        foreach (var stage in score.Stages)
        {
            var stageRow = row.DanStageScoreData.FirstOrDefault(existing => existing.StageIndex == stage.StageIndex);
            if (stageRow is null)
            {
                row.DanStageScoreData.Add(Ac15DaniMapper.ToBlueDanStageScoreDatum(stage, score));
                continue;
            }

            Ac15DaniMapper.ApplyToBlueDanStageScoreDatum(stage, stageRow);
        }
    }

    private static void UpsertGreenStages(DanScoreDatumGreen row, Ac15DaniScore score)
    {
        foreach (var stage in score.Stages)
        {
            var stageRow = row.DanStageScoreData.FirstOrDefault(existing => existing.StageIndex == stage.StageIndex);
            if (stageRow is null)
            {
                row.DanStageScoreData.Add(Ac15DaniMapper.ToGreenDanStageScoreDatum(stage, score));
                continue;
            }

            Ac15DaniMapper.ApplyToGreenDanStageScoreDatum(stage, stageRow);
        }
    }

    private static void UpsertYellowStages(DanScoreDatumYellow row, Ac15DaniScore score)
    {
        foreach (var stage in score.Stages)
        {
            var stageRow = row.DanStageScoreData.FirstOrDefault(existing => existing.StageIndex == stage.StageIndex);
            if (stageRow is null)
            {
                row.DanStageScoreData.Add(Ac15DaniMapper.ToYellowDanStageScoreDatum(stage, score));
                continue;
            }

            Ac15DaniMapper.ApplyToYellowDanStageScoreDatum(stage, stageRow);
        }
    }

    private static Ac15DaniScore BuildUpdatedScore(
        uint baid,
        Ac15DaniScore? existing,
        uint danId,
        bool isExtra,
        uint medleyUniqueId,
        CommonPlayResultData playResultData)
    {
        var existingStages = existing?.Stages.ToDictionary(stage => stage.StageIndex) ?? [];
        var stages = playResultData.AryStageInfoes
            .Select((stage, index) =>
            {
                var stageIndex = (uint)index;
                existingStages.TryGetValue(stageIndex, out var existingStage);
                return BuildUpdatedStage(stageIndex, existingStage, stage);
            })
            .ToArray();

        return new Ac15DaniScore(
            baid,
            danId,
            isExtra,
            existing?.MedleyUniqueId ?? medleyUniqueId,
            Math.Max(existing?.ArrivalSongCount ?? 0, (uint)playResultData.AryStageInfoes.Count),
            Math.Max(existing?.SoulGaugeTotal ?? 0, playResultData.AryStageInfoes.LastOrDefault()?.SoulGauge.GetValueOrDefault() ?? 0),
            Math.Max(existing?.ComboCountTotal ?? 0, playResultData.ComboCntTotal),
            Ac15DanHelpers.ClampGrade(Math.Max((uint)(existing?.ClearGrade ?? Ac15DanClearGrade.NotClear), playResultData.DanResult)),
            stages);
    }

    private static Ac15DaniStageScore BuildUpdatedStage(
        uint stageIndex,
        Ac15DaniStageScore? existing,
        CommonPlayResultData.StageData stage)
    {
        var existingBadCount = existing?.BadCount ?? stage.NgCnt;
        return new Ac15DaniStageScore(
            stageIndex,
            stage.SongNo,
            Math.Max(existing?.PlayScore ?? 0, stage.PlayScore),
            Math.Max(existing?.GoodCount ?? 0, stage.GoodCnt),
            Math.Max(existing?.OkCount ?? 0, stage.OkCnt),
            Math.Min(existingBadCount, stage.NgCnt),
            Math.Max(existing?.DrumrollCount ?? 0, stage.PoundCnt),
            Math.Max(existing?.TotalHitCount ?? 0, stage.HitCnt),
            Math.Max(existing?.ComboCount ?? 0, stage.ComboCnt),
            Math.Max(existing?.HighScore ?? 0, stage.PlayScore));
    }

    private static Ac15DaniSaveUpdate BuildSaveUpdate(
        Ac15DaniScore currentScore,
        IReadOnlyList<Ac15DaniScoreSummary> summaries,
        Ac15DaniSaveState saveState,
        Ac15ProtocolLimits limits,
        Ac15DanClearGrade incomingClearGrade)
    {
        var normalGrades = summaries
            .Where(row => !row.IsExtra && Ac15DanHelpers.IsNormalDanId(row.DanId, limits))
            .ToDictionary(row => row.DanId, row => row.ClearGrade);

        var normalFlags = new byte[limits.DanFlagBytes];
        foreach (var row in summaries.Where(row => !row.IsExtra && Ac15DanHelpers.IsNormalDanId(row.DanId, limits)))
        {
            normalFlags = Ac15DanHelpers.SetPackedGrade(
                normalFlags,
                Ac15DanHelpers.GetPackedIndex(row.DanId, limits),
                row.ClearGrade,
                limits.DanFlagBytes);
        }

        var extraFlags = new byte[limits.DanExtraFlagBytes];
        foreach (var row in summaries.Where(row => row.IsExtra && Ac15DanHelpers.IsExtraDanId(row.DanId, limits)))
        {
            extraFlags = Ac15DanHelpers.SetPackedGrade(
                extraFlags,
                Ac15DanHelpers.GetPackedIndex(row.DanId, limits),
                row.ClearGrade,
                limits.DanExtraFlagBytes);
        }

        var isIncomingClear = Ac15DanHelpers.IsClear(incomingClearGrade);
        var displayDan = !currentScore.IsExtra
                         && Ac15DanHelpers.IsNormalDanId(currentScore.DanId, limits)
                         && isIncomingClear
            ? Ac15DanHelpers.GetDisplayDanAfterNormalClear(currentScore.DanId, limits)
            : Ac15DanHelpers.NormalizeDisplayDan(saveState.DisplayDan, normalGrades, limits);

        return new Ac15DaniSaveUpdate(
            normalFlags,
            extraFlags,
            Ac15DanHelpers.GetGotDanMax(normalGrades, limits),
            displayDan,
            isIncomingClear && saveState.IsAutoCostumeOn,
            saveState.DanCostumeId);
    }
}

public sealed record Ac15DaniSaveState(
    uint Baid,
    uint DisplayDan,
    bool IsAutoCostumeOn,
    uint DanCostumeId);

public sealed record Ac15DaniSaveUpdate(
    byte[] GotDanFlg,
    byte[] GotDanExtraFlg,
    uint GotDanMax,
    uint DisplayDan,
    bool ApplyDanCostume,
    uint DanCostumeId);
