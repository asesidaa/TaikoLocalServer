namespace TaikoLocalServer.Application.Ac15;

public static class Ac15DaniService
{
    public static async ValueTask SaveAsync(
        CommonPlayResultData playResultData,
        Ac15EraProfile profile,
        IEnumerable<Ac15DaniChallenge> challenges,
        IAc15DaniPersistence persistence,
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
        var challenge = challenges.FirstOrDefault(row => row.DanId == danId);
        if (challenge is null
            || !Ac15DanHelpers.IsKnownDanId(danId, limits)
            || !persistence.KnownChallengeLevels.Contains(danId))
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
        var existing = await persistence.GetScoreAsync(key, cancellationToken);
        var updatedScore = BuildUpdatedScore(saveState.Baid, existing, danId, isExtra, challenge.MedleyUniqueId, playResultData);
        var summaries = (await persistence.GetScoreSummariesAsync(saveState.Baid, cancellationToken)).ToList();

        await persistence.UpsertScoreAsync(updatedScore, cancellationToken);

        summaries.RemoveAll(row => row.DanId == updatedScore.DanId && row.IsExtra == updatedScore.IsExtra);
        summaries.Add(new Ac15DaniScoreSummary(updatedScore.DanId, updatedScore.IsExtra, updatedScore.ClearGrade));

        applySaveUpdate(BuildSaveUpdate(updatedScore, summaries, saveState, limits, Ac15DanHelpers.ClampGrade(playResultData.DanResult)));
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
