namespace TaikoLocalServer.Application.Ac15;

public static class Ac15DaniWriter
{
    public static async ValueTask SaveAsync<TScore, TStage>(
        Ac15DaniTables<TScore, TStage> tables,
        CommonPlayResultData playResultData,
        Ac15ProtocolLimits limits,
        IEnumerable<Ac15DaniChallenge> challenges,
        Ac15DaniSaveState saveState,
        Action<Ac15DaniSaveUpdate> applySaveUpdate,
        ILogger logger,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
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
            logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: expected one PlayDan value, got {Count}", saveState.Baid, danIds.Length);
            return;
        }

        if (playResultData.DanResult > (uint)Ac15DanClearGrade.GoldClear)
        {
            logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: invalid DanResult {DanResult}", saveState.Baid, playResultData.DanResult);
            return;
        }

        var danId = danIds[0];
        var knownChallengeLevels = challenges.Select(row => row.DanId).ToHashSet();
        var challenge = challenges.FirstOrDefault(row => row.DanId == danId);
        if (challenge is null
            || !Ac15DanHelpers.IsKnownDanId(danId, limits)
            || !knownChallengeLevels.Contains(danId))
        {
            logger.LogWarning("Skipping AC15 Dani save for baid {Baid}: unknown Dan id {DanId}", saveState.Baid, danId);
            return;
        }

        var isExtra = Ac15DanHelpers.IsExtraDanId(danId, limits);
        var key = new Ac15DaniScoreKey(saveState.Baid, danId, isExtra);
        var existing = await GetScoreAsync(tables, key, cancellationToken);
        var updatedScore = BuildUpdatedScore(saveState.Baid, existing, danId, isExtra, challenge.MedleyUniqueId, playResultData);
        var summaries = (await GetScoreSummariesAsync(tables, saveState.Baid, cancellationToken)).ToList();

        await UpsertScoreAsync(tables, updatedScore, cancellationToken);

        summaries.RemoveAll(row => row.DanId == updatedScore.DanId && row.IsExtra == updatedScore.IsExtra);
        summaries.Add(new Ac15DaniScoreSummary(updatedScore.DanId, updatedScore.IsExtra, updatedScore.ClearGrade));

        applySaveUpdate(BuildSaveUpdate(updatedScore, summaries, saveState, limits, Ac15DanHelpers.ClampGrade(playResultData.DanResult)));
    }

    private static async ValueTask<Ac15DaniScore?> GetScoreAsync<TScore, TStage>(
        Ac15DaniTables<TScore, TStage> tables,
        Ac15DaniScoreKey key,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
        => await tables.ScoresWithStages.SingleOrDefaultAsync(
            score => score.Baid == key.Baid && score.DanId == key.DanId && score.IsExtra == key.IsExtra,
            cancellationToken) is { } row
            ? tables.ToScore(row)
            : null;

    private static async ValueTask<IReadOnlyList<Ac15DaniScoreSummary>> GetScoreSummariesAsync<TScore, TStage>(
        Ac15DaniTables<TScore, TStage> tables,
        uint baid,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
        => (await tables.Scores.Where(row => row.Baid == baid).ToListAsync(cancellationToken))
            .Select(tables.ToSummary)
            .ToArray();

    private static async ValueTask UpsertScoreAsync<TScore, TStage>(
        Ac15DaniTables<TScore, TStage> tables,
        Ac15DaniScore score,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
    {
        var row = await tables.ScoresWithStages.SingleOrDefaultAsync(
            existing => existing.Baid == score.Baid && existing.DanId == score.DanId && existing.IsExtra == score.IsExtra,
            cancellationToken);
        if (row is null)
        {
            row = tables.CreateScore(score);
            UpsertStages(tables.GetStages(row), score, tables.CreateStage, tables.ApplyStage);
            tables.Scores.Add(row);
            return;
        }

        tables.ApplyScore(score, row);
        UpsertStages(tables.GetStages(row), score, tables.CreateStage, tables.ApplyStage);
    }

    private static void UpsertStages<TStage>(
        ICollection<TStage> stageRows,
        Ac15DaniScore score,
        Func<Ac15DaniStageScore, Ac15DaniScore, TStage> createStage,
        Action<Ac15DaniStageScore, TStage> applyStage)
        where TStage : class, IAc15DanStageScoreDatum
    {
        foreach (var stage in score.Stages)
        {
            var stageRow = stageRows.FirstOrDefault(existing => existing.StageIndex == stage.StageIndex);
            if (stageRow is null)
            {
                stageRows.Add(createStage(stage, score));
                continue;
            }

            applyStage(stage, stageRow);
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
