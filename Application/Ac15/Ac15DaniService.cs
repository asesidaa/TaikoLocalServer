using System.Linq.Expressions;

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
            GameEra.Blue => await GetScoresAsync(
                context.DanScoreDataBlue.Include(row => row.DanStageScoreData),
                row => row.Baid == baid && validRequestedIds.Contains(row.DanId),
                Ac15DaniMapper.ToAc15DaniScore,
                cancellationToken),
            GameEra.Green => await GetScoresAsync(
                context.DanScoreDataGreen.Include(row => row.DanStageScoreData),
                row => row.Baid == baid && validRequestedIds.Contains(row.DanId),
                Ac15DaniMapper.ToAc15DaniScore,
                cancellationToken),
            GameEra.Yellow => await GetScoresAsync(
                context.DanScoreDataYellow.Include(row => row.DanStageScoreData),
                row => row.Baid == baid && validRequestedIds.Contains(row.DanId),
                Ac15DaniMapper.ToAc15DaniScore,
                cancellationToken),
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
            GameEra.Blue => await GetScoreAsync(
                context.DanScoreDataBlue.Include(score => score.DanStageScoreData),
                score => score.Baid == key.Baid && score.DanId == key.DanId && score.IsExtra == key.IsExtra,
                Ac15DaniMapper.ToAc15DaniScore,
                cancellationToken),
            GameEra.Green => await GetScoreAsync(
                context.DanScoreDataGreen.Include(score => score.DanStageScoreData),
                score => score.Baid == key.Baid && score.DanId == key.DanId && score.IsExtra == key.IsExtra,
                Ac15DaniMapper.ToAc15DaniScore,
                cancellationToken),
            GameEra.Yellow => await GetScoreAsync(
                context.DanScoreDataYellow.Include(score => score.DanStageScoreData),
                score => score.Baid == key.Baid && score.DanId == key.DanId && score.IsExtra == key.IsExtra,
                Ac15DaniMapper.ToAc15DaniScore,
                cancellationToken),
            _ => null
        };

    private static async ValueTask<IReadOnlyList<Ac15DaniScoreSummary>> GetScoreSummariesAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        CancellationToken cancellationToken)
        => profile.Era switch
        {
            GameEra.Blue => await GetScoreSummariesAsync(
                context.DanScoreDataBlue,
                row => row.Baid == baid,
                Ac15DaniMapper.ToAc15DaniScoreSummary,
                cancellationToken),
            GameEra.Green => await GetScoreSummariesAsync(
                context.DanScoreDataGreen,
                row => row.Baid == baid,
                Ac15DaniMapper.ToAc15DaniScoreSummary,
                cancellationToken),
            GameEra.Yellow => await GetScoreSummariesAsync(
                context.DanScoreDataYellow,
                row => row.Baid == baid,
                Ac15DaniMapper.ToAc15DaniScoreSummary,
                cancellationToken),
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
                await UpsertScoreAsync(
                    context.DanScoreDataBlue,
                    context.DanScoreDataBlue.Include(existing => existing.DanStageScoreData),
                    score,
                    existing => existing.Baid == score.Baid && existing.DanId == score.DanId && existing.IsExtra == score.IsExtra,
                    Ac15DaniMapper.ToBlueDanScoreDatum,
                    Ac15DaniMapper.ApplyToBlueDanScoreDatum,
                    row => row.DanStageScoreData,
                    Ac15DaniMapper.ToBlueDanStageScoreDatum,
                    Ac15DaniMapper.ApplyToBlueDanStageScoreDatum,
                    cancellationToken);
                return;
            case GameEra.Green:
                await UpsertScoreAsync(
                    context.DanScoreDataGreen,
                    context.DanScoreDataGreen.Include(existing => existing.DanStageScoreData),
                    score,
                    existing => existing.Baid == score.Baid && existing.DanId == score.DanId && existing.IsExtra == score.IsExtra,
                    Ac15DaniMapper.ToGreenDanScoreDatum,
                    Ac15DaniMapper.ApplyToGreenDanScoreDatum,
                    row => row.DanStageScoreData,
                    Ac15DaniMapper.ToGreenDanStageScoreDatum,
                    Ac15DaniMapper.ApplyToGreenDanStageScoreDatum,
                    cancellationToken);
                return;
            case GameEra.Yellow:
                await UpsertScoreAsync(
                    context.DanScoreDataYellow,
                    context.DanScoreDataYellow.Include(existing => existing.DanStageScoreData),
                    score,
                    existing => existing.Baid == score.Baid && existing.DanId == score.DanId && existing.IsExtra == score.IsExtra,
                    Ac15DaniMapper.ToYellowDanScoreDatum,
                    Ac15DaniMapper.ApplyToYellowDanScoreDatum,
                    row => row.DanStageScoreData,
                    Ac15DaniMapper.ToYellowDanStageScoreDatum,
                    Ac15DaniMapper.ApplyToYellowDanStageScoreDatum,
                    cancellationToken);
                return;
        }
    }

    private static async ValueTask<IReadOnlyList<Ac15DaniScore>> GetScoresAsync<TScore>(
        IQueryable<TScore> scores,
        Expression<Func<TScore, bool>> filter,
        Func<TScore, Ac15DaniScore> map,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        => (await scores
                .Where(filter)
                .ToListAsync(cancellationToken))
            .OrderBy(row => row.DanId)
            .Select(map)
            .ToArray();

    private static async ValueTask<Ac15DaniScore?> GetScoreAsync<TScore>(
        IQueryable<TScore> scores,
        Expression<Func<TScore, bool>> filter,
        Func<TScore, Ac15DaniScore> map,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        => await scores.SingleOrDefaultAsync(filter, cancellationToken) is { } row
            ? map(row)
            : null;

    private static async ValueTask<IReadOnlyList<Ac15DaniScoreSummary>> GetScoreSummariesAsync<TScore>(
        DbSet<TScore> scores,
        Expression<Func<TScore, bool>> filter,
        Func<TScore, Ac15DaniScoreSummary> map,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        => (await scores
                .Where(filter)
                .ToListAsync(cancellationToken))
            .Select(map)
            .ToArray();

    private static async ValueTask UpsertScoreAsync<TScore, TStage>(
        DbSet<TScore> scores,
        IQueryable<TScore> scoresWithStages,
        Ac15DaniScore score,
        Expression<Func<TScore, bool>> filter,
        Func<Ac15DaniScore, TScore> createScore,
        Action<Ac15DaniScore, TScore> applyScore,
        Func<TScore, ICollection<TStage>> getStages,
        Func<Ac15DaniStageScore, Ac15DaniScore, TStage> createStage,
        Action<Ac15DaniStageScore, TStage> applyStage,
        CancellationToken cancellationToken)
        where TScore : class, IAc15DanScoreDatum
        where TStage : class, IAc15DanStageScoreDatum
    {
        var row = await scoresWithStages.SingleOrDefaultAsync(filter, cancellationToken);
        if (row is null)
        {
            row = createScore(score);
            UpsertStages(getStages(row), score, createStage, applyStage);
            scores.Add(row);
            return;
        }

        applyScore(score, row);
        UpsertStages(getStages(row), score, createStage, applyStage);
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
