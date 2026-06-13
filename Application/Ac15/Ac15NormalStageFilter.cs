using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15NormalStagePolicy(
    Func<Ac15StageResult, Ac15StageSupportDecision> IsSupported,
    Func<Ac15StageResult, CrownType, Ac15BestUpdatePolicy> GetBestUpdatePolicy);

public static class Ac15NormalStageFilter
{
    public static IReadOnlyList<Ac15StageResult> Filter(
        uint baid,
        IEnumerable<Ac15StageResult> stages,
        Ac15ProtocolLimits limits,
        Ac15NormalStagePolicy policy,
        ILogger logger)
    {
        var accepted = new List<Ac15StageResult>();
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
