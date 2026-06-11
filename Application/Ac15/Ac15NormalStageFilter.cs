namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15NormalStagePolicy(
    Func<CommonPlayResultData.StageData, Ac15StageSupportDecision> IsSupported,
    Func<CommonPlayResultData.StageData, CrownType, Ac15BestUpdatePolicy> GetBestUpdatePolicy);

public static class Ac15NormalStageFilter
{
    public static IReadOnlyList<CommonPlayResultData.StageData> Filter(
        uint baid,
        IEnumerable<CommonPlayResultData.StageData> stages,
        Ac15ProtocolLimits limits,
        Ac15NormalStagePolicy policy,
        ILogger logger)
    {
        var accepted = new List<CommonPlayResultData.StageData>();
        foreach (var stage in stages)
        {
            if (stage.SongNo >= limits.SongFlagBytes * 8
                || stage.Level < limits.MinCourseLevel
                || stage.Level > limits.MaxCourseLevel)
            {
                logger.LogWarning(
                    "Skipping invalid AC15 stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode}",
                    baid,
                    stage.SongNo,
                    stage.Level,
                    stage.StageMode);
                continue;
            }

            var decision = policy.IsSupported(stage);
            if (!decision.IsSupported)
            {
                logger.LogWarning(
                    "Skipping unsupported AC15 stage for baid {Baid}: song={SongNo} level={Level} stage_mode={StageMode} reason={Reason}",
                    baid,
                    stage.SongNo,
                    stage.Level,
                    stage.StageMode,
                    decision.Reason ?? "unsupported");
                continue;
            }

            accepted.Add(stage);
        }

        return accepted;
    }
}
