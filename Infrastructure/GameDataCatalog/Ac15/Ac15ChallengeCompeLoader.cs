using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Ac15.ChallengeCompe;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15ChallengeCompeLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task<Ac15ChallengeCompeCatalog> LoadFromFileAsync(
        string path,
        string eraName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(path))
        {
            return Ac15ChallengeCompeCatalog.Disabled;
        }

        RawChallengeCompeCatalog raw;
        try
        {
            await using var stream = File.OpenRead(path);
            raw = await JsonSerializer.DeserializeAsync<RawChallengeCompeCatalog>(stream, JsonOptions, cancellationToken)
                  ?? new RawChallengeCompeCatalog();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"{eraName} ChallengeCompe data is malformed: {path}", ex);
        }

        if (!raw.Enabled)
        {
            return Ac15ChallengeCompeCatalog.Disabled;
        }

        var bundles = (raw.MonthlyBundles ?? [])
            .Select((bundle, index) => MapBundle(bundle, index, eraName))
            .ToArray();

        var duplicateIds = bundles
            .GroupBy(bundle => bundle.BundleId, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicateIds.Length > 0)
        {
            throw new InvalidDataException($"{eraName} ChallengeCompe data contains duplicate bundle_id values: {string.Join(", ", duplicateIds)}");
        }

        return new Ac15ChallengeCompeCatalog(true, bundles);
    }

    private static Ac15ChallengeCompeMonthlyBundle MapBundle(
        RawMonthlyBundle raw,
        int index,
        string eraName)
    {
        var rowNumber = index + 1;
        var bundleId = raw.BundleId?.Trim();
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            throw new InvalidDataException($"{eraName} ChallengeCompe bundle {rowNumber} is missing bundle_id.");
        }

        var personalTasks = (raw.PersonalTasks ?? [])
            .Select((task, taskIndex) => MapPersonalTask(bundleId, task, taskIndex, eraName))
            .ToArray();
        if (personalTasks.Length != Ac15ChallengeCompeMonthlyBundle.ExpectedPersonalTaskCount)
        {
            throw new InvalidDataException(
                $"{eraName} ChallengeCompe bundle {bundleId} must contain exactly {Ac15ChallengeCompeMonthlyBundle.ExpectedPersonalTaskCount} personal tasks.");
        }

        var duplicateSlots = personalTasks
            .GroupBy(task => task.Slot)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicateSlots.Length > 0)
        {
            throw new InvalidDataException($"{eraName} ChallengeCompe bundle {bundleId} contains duplicate personal task slots: {string.Join(", ", duplicateSlots)}");
        }

        return new Ac15ChallengeCompeMonthlyBundle(
            bundleId,
            MapDate(raw.StartsAt, bundleId, "starts_at", eraName),
            MapDate(raw.EndsAt, bundleId, "ends_at", eraName),
            personalTasks,
            raw.CommunityTask is null ? null : MapCommunityTask(bundleId, raw.CommunityTask, eraName),
            (raw.Rewards ?? []).Select(reward => MapReward(bundleId, reward, eraName)).ToArray());
    }

    private static Ac15ChallengeCompeTask MapPersonalTask(
        string bundleId,
        RawTask raw,
        int index,
        string eraName)
    {
        var slot = raw.Slot ?? (uint)(index + 1);
        if (slot is 0 or > Ac15ChallengeCompeMonthlyBundle.ExpectedPersonalTaskCount)
        {
            throw new InvalidDataException($"{eraName} ChallengeCompe bundle {bundleId} has unsupported personal task slot {slot}.");
        }

        return new Ac15ChallengeCompeTask(
            raw.TaskId,
            slot,
            raw.Name?.Trim() ?? string.Empty,
            MapRule(raw.Rule));
    }

    private static Ac15ChallengeCompeCommunityTask MapCommunityTask(
        string bundleId,
        RawCommunityTask raw,
        string eraName)
    {
        if (raw.TaskId == 0)
        {
            throw new InvalidDataException($"{eraName} ChallengeCompe bundle {bundleId} community task is missing task_id.");
        }

        return new Ac15ChallengeCompeCommunityTask(
            raw.TaskId,
            raw.Name?.Trim() ?? string.Empty,
            MapRule(raw.Rule),
            raw.Description?.Trim());
    }

    private static Ac15ChallengeCompeRule MapRule(RawRule? raw)
    {
        if (raw is null)
        {
            return Ac15ChallengeCompeRule.Unsupported;
        }

        return new Ac15ChallengeCompeRule(
            MapRuleKind(raw.Kind),
            raw.Threshold,
            raw.SongNoes ?? []);
    }

    private static Ac15ChallengeCompeRuleKind MapRuleKind(string? value)
    {
        var normalized = value?.Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Trim()
            .ToLowerInvariant();

        return normalized switch
        {
            "clear" => Ac15ChallengeCompeRuleKind.Clear,
            "fullcombo" => Ac15ChallengeCompeRuleKind.FullCombo,
            "scorethreshold" => Ac15ChallengeCompeRuleKind.ScoreThreshold,
            "songsetcount" => Ac15ChallengeCompeRuleKind.SongSetCount,
            "communitycount" => Ac15ChallengeCompeRuleKind.CommunityCount,
            _ => Ac15ChallengeCompeRuleKind.Unsupported
        };
    }

    private static Ac15ChallengeCompeReward MapReward(
        string bundleId,
        RawReward raw,
        string eraName)
    {
        if (raw.RequiredCompletedTasks == 0)
        {
            throw new InvalidDataException($"{eraName} ChallengeCompe bundle {bundleId} reward is missing required_completed_tasks.");
        }

        return new Ac15ChallengeCompeReward(
            raw.RequiredCompletedTasks,
            raw.RewardSongNoes ?? [],
            raw.RewardTitleIds ?? []);
    }

    private static DateTimeOffset? MapDate(
        string? value,
        string bundleId,
        string fieldName,
        string eraName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, out var parsed))
        {
            return parsed;
        }

        throw new InvalidDataException($"{eraName} ChallengeCompe bundle {bundleId} has invalid {fieldName} value.");
    }

    private sealed class RawChallengeCompeCatalog
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("monthly_bundles")]
        public RawMonthlyBundle[]? MonthlyBundles { get; set; }
    }

    private sealed class RawMonthlyBundle
    {
        [JsonPropertyName("bundle_id")]
        public string? BundleId { get; set; }

        [JsonPropertyName("starts_at")]
        public string? StartsAt { get; set; }

        [JsonPropertyName("ends_at")]
        public string? EndsAt { get; set; }

        [JsonPropertyName("personal_tasks")]
        public RawTask[]? PersonalTasks { get; set; }

        [JsonPropertyName("community_task")]
        public RawCommunityTask? CommunityTask { get; set; }

        [JsonPropertyName("rewards")]
        public RawReward[]? Rewards { get; set; }
    }

    private sealed class RawTask
    {
        [JsonPropertyName("task_id")]
        public uint TaskId { get; set; }

        [JsonPropertyName("slot")]
        public uint? Slot { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("rule")]
        public RawRule? Rule { get; set; }
    }

    private sealed class RawCommunityTask
    {
        [JsonPropertyName("task_id")]
        public uint TaskId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("rule")]
        public RawRule? Rule { get; set; }
    }

    private sealed class RawRule
    {
        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("threshold")]
        public uint? Threshold { get; set; }

        [JsonPropertyName("song_no")]
        public uint[]? SongNoes { get; set; }
    }

    private sealed class RawReward
    {
        [JsonPropertyName("required_completed_tasks")]
        public uint RequiredCompletedTasks { get; set; }

        [JsonPropertyName("reward_song_no")]
        public uint[]? RewardSongNoes { get; set; }

        [JsonPropertyName("reward_title_id")]
        public uint[]? RewardTitleIds { get; set; }
    }
}
