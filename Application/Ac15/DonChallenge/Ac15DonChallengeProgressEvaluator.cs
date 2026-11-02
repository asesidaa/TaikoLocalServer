using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15.DonChallenge;

public sealed record Ac15DonChallengeStageEvaluation(
    string BundleId,
    Ac15DonChallengeTask Task,
    Ac15DonChallengeTrackDefinition Track,
    Ac15StageResult Stage,
    uint ProgressValue,
    bool Completed);

public static class Ac15DonChallengeProgressEvaluator
{
    public static IReadOnlyList<Ac15DonChallengeStageEvaluation> Evaluate(
        Ac15DonChallengeCatalog catalog,
        IReadOnlyList<Ac15StageResult> stages)
    {
        var activeTasks = catalog.GetActiveBundles()
            .SelectMany(bundle => bundle.PersonalTasks
                .Where(task => task.Rule.CanExecute)
                .SelectMany(task => Ac15DonChallengeTrackDefinitions.FromTask(task)
                    .Select(track => new ActiveTask(bundle.BundleId, task, track))))
            .ToArray();
        if (activeTasks.Length == 0)
        {
            return [];
        }

        var matches = new List<Ac15DonChallengeStageEvaluation>();
        foreach (var stage in stages)
        {
            foreach (var activeTask in activeTasks)
            {
                if (!activeTask.Track.Matches(stage)
                    || !TryEvaluate(activeTask.Task.Rule, stage, out var progressValue, out var completed))
                {
                    continue;
                }

                matches.Add(new Ac15DonChallengeStageEvaluation(
                    activeTask.BundleId,
                    activeTask.Task,
                    activeTask.Track,
                    stage,
                    progressValue,
                    completed));
            }
        }

        return matches;
    }

    private static bool TryEvaluate(
        Ac15DonChallengeRule rule,
        Ac15StageResult stage,
        out uint progressValue,
        out bool completed)
    {
        progressValue = 0;
        completed = false;

        switch (rule.Kind)
        {
            case Ac15DonChallengeRuleKind.Clear:
                if (stage.PlayResult == 0 || !rule.AllowsStage(stage))
                {
                    return false;
                }

                progressValue = 1;
                completed = !rule.RequiresDistinctSongProgress;
                return true;

            case Ac15DonChallengeRuleKind.FullCombo:
                if (stage.PlayResult < 2 || !rule.AllowsStage(stage))
                {
                    return false;
                }

                progressValue = 1;
                completed = !rule.RequiresDistinctSongProgress;
                return true;

            case Ac15DonChallengeRuleKind.ScoreThreshold:
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

    private sealed record ActiveTask(
        string BundleId,
        Ac15DonChallengeTask Task,
        Ac15DonChallengeTrackDefinition Track);
}
