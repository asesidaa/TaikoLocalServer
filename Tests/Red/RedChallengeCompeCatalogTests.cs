using System.Runtime.CompilerServices;
using TaikoLocalServer.Application.Ac15.ChallengeCompe;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Red;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedChallengeCompeCatalogTests
{
    [Fact]
    public async Task DisabledConfigProducesNoActiveTasks()
    {
        var catalog = await LoadJsonAsync("""
        {
          "enabled": false,
          "monthly_bundles": [
            {
              "bundle_id": "",
              "personal_tasks": []
            }
          ]
        }
        """);

        Assert.False(catalog.Enabled);
        Assert.Empty(catalog.GetActiveBundles(DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task ValidActiveBundleLoadsTypedRulesAndRewards()
    {
        var catalog = await LoadJsonAsync(BuildEnabledCatalog());

        var bundle = Assert.Single(catalog.GetActiveBundles(DateTimeOffset.Parse("2016-07-20T00:00:00Z")));

        Assert.True(catalog.Enabled);
        Assert.Equal("red-2016-07", bundle.BundleId);
        Assert.True(bundle.HasExpectedPersonalTaskCount);
        Assert.Equal(Ac15ChallengeCompeRuleKind.Clear, bundle.PersonalTasks[0].Rule.Kind);
        Assert.True(bundle.PersonalTasks[0].Rule.CanExecute);
        Assert.Equal(Ac15ChallengeCompeRuleKind.FullCombo, bundle.PersonalTasks[1].Rule.Kind);
        Assert.Equal(Ac15ChallengeCompeRuleKind.ScoreThreshold, bundle.PersonalTasks[2].Rule.Kind);
        Assert.Equal(765000u, bundle.PersonalTasks[2].Rule.Threshold);
        Assert.Equal(Ac15ChallengeCompeRuleKind.SongSetCount, bundle.PersonalTasks[3].Rule.Kind);
        Assert.Equal([101u, 102u, 103u], bundle.PersonalTasks[3].Rule.SongNoes);
        Assert.Equal(Ac15ChallengeCompeRuleKind.CommunityCount, bundle.CommunityTask?.Rule.Kind);
        Assert.Equal(100000u, bundle.CommunityTask?.Rule.Threshold);
        Assert.Collection(
            bundle.Rewards,
            reward =>
            {
                Assert.Equal(8u, reward.RequiredCompletedTasks);
                Assert.Equal([700u], reward.RewardSongNoes);
                Assert.Empty(reward.RewardTitleIds);
            },
            reward =>
            {
                Assert.Equal(10u, reward.RequiredCompletedTasks);
                Assert.Empty(reward.RewardSongNoes);
                Assert.Equal([3001u], reward.RewardTitleIds);
            });
    }

    [Fact]
    public async Task UnknownRuleTypeDeserializesUnsupportedAndCannotExecute()
    {
        var catalog = await LoadJsonAsync(BuildEnabledCatalog(firstRuleKind: "world_domination"));

        var bundle = Assert.Single(catalog.GetActiveBundles(DateTimeOffset.Parse("2016-07-20T00:00:00Z")));
        var firstRule = bundle.PersonalTasks[0].Rule;

        Assert.Equal(Ac15ChallengeCompeRuleKind.Unsupported, firstRule.Kind);
        Assert.False(firstRule.CanExecute);
    }

    [Fact]
    public async Task CommittedRedSidecarExistsAndLoadsDisabled()
    {
        var path = Path.Combine(
            RepoRoot(),
            "Host",
            "wwwroot",
            "data",
            "red",
            RedEraGameDataCatalog.ChallengeCompeFileName);

        var catalog = await Ac15ChallengeCompeLoader.LoadFromFileAsync(
            path,
            nameof(GameEra.Red),
            CancellationToken.None);

        Assert.False(catalog.Enabled);
        Assert.Empty(catalog.GetActiveBundles(DateTimeOffset.UtcNow));
    }

    private static async Task<Ac15ChallengeCompeCatalog> LoadJsonAsync(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path, json, CancellationToken.None);
            return await Ac15ChallengeCompeLoader.LoadFromFileAsync(
                path,
                nameof(GameEra.Red),
                CancellationToken.None);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string BuildEnabledCatalog(string firstRuleKind = "clear")
    {
        var tasks = string.Join(
            "," + Environment.NewLine,
            Enumerable.Range(1, 10).Select(index => index switch
            {
                1 => $$"""{ "task_id": 1001, "slot": 1, "name": "Clear one song", "rule": { "kind": "{{firstRuleKind}}" } }""",
                2 => """{ "task_id": 1002, "slot": 2, "name": "Full combo", "rule": { "kind": "full_combo" } }""",
                3 => """{ "task_id": 1003, "slot": 3, "name": "Score target", "rule": { "kind": "score_threshold", "threshold": 765000 } }""",
                4 => """{ "task_id": 1004, "slot": 4, "name": "Song set", "rule": { "kind": "song_set_count", "threshold": 2, "song_no": [101, 102, 103] } }""",
                _ => $$"""{ "task_id": {{1000 + index}}, "slot": {{index}}, "name": "Clear task {{index}}", "rule": { "kind": "clear" } }"""
            }));

        return $$"""
        {
          "enabled": true,
          "monthly_bundles": [
            {
              "bundle_id": "red-2016-07",
              "starts_at": "2016-07-14T00:00:00Z",
              "ends_at": "2016-08-15T00:00:00Z",
              "personal_tasks": [
        {{tasks}}
              ],
              "community_task": {
                "task_id": 9001,
                "name": "Community clears",
                "description": "Community target metadata only",
                "rule": { "kind": "community_count", "threshold": 100000 }
              },
              "rewards": [
                { "required_completed_tasks": 8, "reward_song_no": [700], "reward_title_id": [] },
                { "required_completed_tasks": 10, "reward_song_no": [], "reward_title_id": [3001] }
              ]
            }
          ]
        }
        """;
    }

    private static string RepoRoot([CallerFilePath] string sourceFilePath = "")
    {
        for (var directory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath)!);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
