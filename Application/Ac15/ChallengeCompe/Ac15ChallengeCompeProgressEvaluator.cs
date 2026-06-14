using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15.ChallengeCompe;

public sealed record Ac15ChallengeCompeStageEvaluation(
    string BundleId,
    Ac15ChallengeCompeTask Task,
    Ac15CompeIdFact Fact,
    Ac15StageResult Stage,
    uint ProgressValue,
    bool Completed);

public static class Ac15ChallengeCompeProgressEvaluator
{
    public static IReadOnlyList<Ac15ChallengeCompeStageEvaluation> Evaluate(
        Ac15ChallengeCompeCatalog catalog,
        IReadOnlyList<Ac15StageResult> stages)
    {
        var activeTasks = catalog.GetActiveBundles()
            .SelectMany(bundle => bundle.PersonalTasks
                .Where(task => task.Rule.CanExecute)
                .Select(task => new ActiveTask(bundle.BundleId, task)))
            .ToArray();
        if (activeTasks.Length == 0)
        {
            return [];
        }

        var matches = new List<Ac15ChallengeCompeStageEvaluation>();
        foreach (var stage in stages)
        {
            foreach (var fact in stage.ChallengeIds)
            {
                var activeTask = activeTasks.FirstOrDefault(task =>
                    task.Task.CompeId == fact.CompeId && task.Task.TrackNo == fact.TrackNo);
                if (activeTask is null || !TryEvaluate(activeTask.Task.Rule, stage, out var progressValue, out var completed))
                {
                    continue;
                }

                matches.Add(new Ac15ChallengeCompeStageEvaluation(
                    activeTask.BundleId,
                    activeTask.Task,
                    fact,
                    stage,
                    progressValue,
                    completed));
            }
        }

        return matches;
    }

    private static bool TryEvaluate(
        Ac15ChallengeCompeRule rule,
        Ac15StageResult stage,
        out uint progressValue,
        out bool completed)
    {
        progressValue = 0;
        completed = false;

        switch (rule.Kind)
        {
            case Ac15ChallengeCompeRuleKind.Clear:
                if (stage.PlayResult == 0 || !rule.AllowsStage(stage))
                {
                    return false;
                }

                progressValue = 1;
                completed = !rule.RequiresDistinctSongProgress;
                return true;

            case Ac15ChallengeCompeRuleKind.FullCombo:
                if (stage.PlayResult < 2 || !rule.AllowsStage(stage))
                {
                    return false;
                }

                progressValue = 1;
                completed = !rule.RequiresDistinctSongProgress;
                return true;

            case Ac15ChallengeCompeRuleKind.ScoreThreshold:
                if (!rule.AllowsStage(stage))
                {
                    return false;
                }

                progressValue = stage.PlayScore;
                completed = rule.MinimumScore is { } minimumScore && stage.PlayScore >= minimumScore;
                return completed;

            default:
                return false;
        }
    }

    private sealed record ActiveTask(string BundleId, Ac15ChallengeCompeTask Task);
}
