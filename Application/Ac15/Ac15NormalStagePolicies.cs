using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15NormalStagePolicies
{
    public static Ac15NormalStagePolicy Standard { get; } = new(
        stage => stage.StageMode is 0 or 1
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not a standard normal stage"),
        (_, _) => new Ac15BestUpdatePolicy(AllowScoreUpdate: true, AllowCrownUpdate: true));

    public static Ac15NormalStagePolicy Green { get; } = new(
        stage => stage.StageMode is 0 or 1 or 3 or 4
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not supported by Green"),
        (stage, _) =>
        {
            var isAiBattle = GreenStageModeInterpreter.IsAiBattle(stage.StageMode);
            var allowCrownUpdate = !isAiBattle || GreenAiBattleLevels.AllowsCrown(stage.Level, stage.SupportLevel);
            return new Ac15BestUpdatePolicy(AllowScoreUpdate: true, AllowCrownUpdate: allowCrownUpdate);
        });
}
