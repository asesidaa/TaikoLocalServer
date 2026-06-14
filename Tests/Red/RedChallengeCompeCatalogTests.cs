using System.Runtime.CompilerServices;
using System.Text.Json;
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
          "monthly_bundles": []
        }
        """);

        Assert.False(catalog.Enabled);
        Assert.Empty(catalog.GetActiveBundles());
    }

    [Fact]
    public async Task ValidActiveBundleLoadsTypedRulesAndRewards()
    {
        var catalog = await LoadJsonAsync(BuildEnabledCatalog());

        var bundle = Assert.Single(catalog.GetActiveBundles());

        Assert.True(catalog.Enabled);
        Assert.Equal("red-2016-07", bundle.BundleId);
        Assert.True(bundle.HasExpectedPersonalTaskCount);
        Assert.Equal(Ac15ChallengeCompeRuleKind.Clear, bundle.PersonalTasks[0].Rule.Kind);
        Assert.Equal(1u, bundle.PersonalTasks[0].Rule.RequiredSongCount);
        Assert.True(bundle.PersonalTasks[0].Rule.CanExecute);
        Assert.Equal(Ac15ChallengeCompeRuleKind.FullCombo, bundle.PersonalTasks[1].Rule.Kind);
        Assert.Equal(3u, bundle.PersonalTasks[1].Rule.MinimumLevel);
        Assert.Equal(Ac15ChallengeCompeRuleKind.ScoreThreshold, bundle.PersonalTasks[2].Rule.Kind);
        Assert.Equal(765000u, bundle.PersonalTasks[2].Rule.MinimumScore);
        Assert.Equal(Ac15ChallengeCompeRuleKind.Clear, bundle.PersonalTasks[3].Rule.Kind);
        Assert.Equal(2u, bundle.PersonalTasks[3].Rule.RequiredSongCount);
        Assert.Equal([101u, 102u, 103u], bundle.PersonalTasks[3].Rule.EligibleSongNoes);
        Assert.Equal(Ac15ChallengeCompeRuleKind.CommunityCount, bundle.CommunityTask?.Rule.Kind);
        Assert.Equal(100000u, bundle.CommunityTask?.Rule.RequiredCommunityCount);
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
    public async Task UnknownRuleTypeFailsSchemaValidation()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            LoadJsonAsync(BuildEnabledCatalog(firstRuleKind: "world_domination")));

        Assert.Contains("schema", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LegacyThresholdRuleFailsSchemaValidation()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            LoadJsonAsync(BuildEnabledCatalog(firstRule: """{ "kind": "clear", "threshold": 1 }""")));

        Assert.Contains("schema", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CommittedRedSidecarExistsAndLoadsWikiBundles()
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
            isEnabled: true,
            activeBundleId: "red-2016-08",
            nameof(GameEra.Red),
            CancellationToken.None);

        var activeBundle = Assert.Single(catalog.GetActiveBundles());

        Assert.True(catalog.Enabled);
        Assert.Equal(7, catalog.MonthlyBundles.Count);
        Assert.All(catalog.MonthlyBundles, bundle => Assert.True(bundle.HasExpectedPersonalTaskCount));

        Assert.Equal("red-2016-08", activeBundle.BundleId);
        var august = Assert.Single(catalog.MonthlyBundles, bundle => bundle.BundleId == "red-2016-08");
        Assert.Equal(5000u, august.CommunityTask?.Rule.RequiredCommunityCount);
        Assert.Equal([618u], august.CommunityTask?.Rule.EligibleSongNoes);
        Assert.Equal([618u], august.Rewards[0].RewardSongNoes);
        Assert.Equal([484u], august.Rewards[1].RewardTitleIds);
        Assert.Equal(3u, august.PersonalTasks[2].Rule.MinimumLevel);

        var february = Assert.Single(catalog.MonthlyBundles, bundle => bundle.BundleId == "red-2017-02");
        Assert.Equal([388u, 677u], february.PersonalTasks[9].Rule.EligibleSongNoes);
        Assert.Equal(2u, february.PersonalTasks[9].Rule.RequiredSongCount);
        Assert.Equal([664u], february.Rewards[0].RewardSongNoes);
        Assert.Equal([521u], february.Rewards[1].RewardTitleIds);
    }

    [Fact]
    public void ChallengeCompeSchemaIsVisibleInBuildOutput()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "schemas",
            "ac15-challenge-compe-catalog.schema.json");

        Assert.True(File.Exists(path), $"Expected ChallengeCompe schema in build output at {path}.");

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(
            "https://json-schema.org/draft/2020-12/schema",
            document.RootElement.GetProperty("$schema").GetString());
    }

    private static async Task<Ac15ChallengeCompeCatalog> LoadJsonAsync(
        string json,
        bool isEnabled = true,
        string? activeBundleId = "red-2016-07")
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path, json, CancellationToken.None);
            return await Ac15ChallengeCompeLoader.LoadFromFileAsync(
                path,
                isEnabled,
                activeBundleId,
                nameof(GameEra.Red),
                CancellationToken.None);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string BuildEnabledCatalog(string firstRuleKind = "clear", string? firstRule = null)
    {
        var firstRuleJson = firstRule ?? $$"""{ "kind": "{{firstRuleKind}}", "required_song_count": 1 }""";
        var tasks = string.Join(
            "," + Environment.NewLine,
            Enumerable.Range(1, 10).Select(index => index switch
            {
                1 => $$"""{ "task_id": 1001, "slot": 1, "name": "Clear one song", "rule": {{firstRuleJson}} }""",
                2 => """{ "task_id": 1002, "slot": 2, "name": "Full combo", "rule": { "kind": "full_combo", "required_song_count": 1, "minimum_level": 3 } }""",
                3 => """{ "task_id": 1003, "slot": 3, "name": "Score target", "rule": { "kind": "score_threshold", "minimum_score": 765000 } }""",
                4 => """{ "task_id": 1004, "slot": 4, "name": "Song set", "rule": { "kind": "clear", "required_song_count": 2, "eligible_song_noes": [101, 102, 103] } }""",
                _ => $$"""{ "task_id": {{1000 + index}}, "slot": {{index}}, "name": "Clear task {{index}}", "rule": { "kind": "clear", "required_song_count": 1 } }"""
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
                "rule": { "kind": "community_count", "required_community_count": 100000, "eligible_song_noes": [700] }
              },
              "rewards": [
                { "required_completed_tasks": 8, "reward_song_noes": [700], "reward_title_ids": [] },
                { "required_completed_tasks": 10, "reward_song_noes": [], "reward_title_ids": [3001] }
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
