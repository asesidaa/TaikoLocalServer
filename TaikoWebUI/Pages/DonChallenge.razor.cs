using System.Globalization;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Domain.Enums;
using TaikoWebUI.Utilities;

namespace TaikoWebUI.Pages;

public partial class DonChallenge
{
    [Parameter]
    public int Baid { get; set; }

    [Parameter]
    public string? Era { get; set; }

    private DonChallengeResponse? response;
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private bool isLoading;
    private bool isError;

    private bool IsUnauthorized
        => AuthService.LoginRequired
           && (!AuthService.IsLoggedIn || (AuthService.GetLoggedInBaid() != Baid && !AuthService.IsAdmin));

    private string RequestedEra
        => string.IsNullOrWhiteSpace(Era) ? AuthService.DefaultEra : Era.Trim();

    private string CurrentEra
        => WebUiEra.TryNormalize(RequestedEra, out var normalized)
            ? normalized
            : RequestedEra;

    private bool CanLoadCurrentEra
        => WebUiEra.TryNormalize(RequestedEra, out var normalized)
           && WebUiEra.SupportsOlderAc15DonChallenge(normalized);

    private IEnumerable<DonChallengeTask> OrderedTasks
        => response?.Tasks.OrderBy(task => task.Slot).ThenBy(task => task.TaskId)
           ?? Enumerable.Empty<DonChallengeTask>();

    private IEnumerable<DonChallengeReward> OrderedRewards
        => response?.Rewards.OrderBy(reward => reward.RequiredCompletedTasks)
           ?? Enumerable.Empty<DonChallengeReward>();

    private uint ConfiguredRewardCount
        => response?.Rewards.Aggregate(0u, (total, reward) =>
            total + (uint)reward.RewardSongNoes.Count + (uint)reward.RewardTitleIds.Count) ?? 0;

    private uint EarnedRewardCount
        => response?.Rewards
               .Where(reward => reward.Status == DonChallengeRewardStatus.Earned)
               .Aggregate(0u, (total, reward) =>
                   total + (uint)reward.RewardSongNoes.Count + (uint)reward.RewardTitleIds.Count)
           ?? 0;

    private string UnavailableHeading
        => IsNoActiveBundle ? "No active Don Challenge" : "Don Challenge unavailable";

    private string UnavailableBody
        => IsNoActiveBundle
            ? "No active Don Challenge is configured for this era."
            : $"Don Challenge is not available for {CurrentEra}.";

    private bool IsNoActiveBundle
        => response?.Message?.StartsWith("No active Don Challenge", StringComparison.OrdinalIgnoreCase) == true;

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        isLoading = true;
        isError = false;
        response = null;
        musicDetailDictionary = new Dictionary<uint, MusicDetail>();
        titleDictionary = new Dictionary<uint, Title>();
        BuildBreadcrumbs();

        if (IsUnauthorized)
        {
            isLoading = false;
            return;
        }

        if (!CanLoadCurrentEra)
        {
            response = new DonChallengeResponse
            {
                Era = CurrentEra,
                IsAvailable = false,
                Message = $"Don Challenge is not available for {CurrentEra}."
            };
            isLoading = false;
            return;
        }

        try
        {
            response = await DonChallengeService.GetDonChallengeAsync(CurrentEra, Baid);
            if (response.IsAvailable)
            {
                musicDetailDictionary = await GameDataService.GetMusicDetailDictionary(CurrentEra);
                titleDictionary = await GameDataService.GetTitleDictionary(CurrentEra);
            }
        }
        catch
        {
            isError = true;
        }
        finally
        {
            isLoading = false;
        }
    }

    private void BuildBreadcrumbs()
    {
        BreadcrumbsStateContainer.breadcrumbs.Clear();
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin)
        {
            BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        }
        else
        {
            BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        }

        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"User {Baid}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(
            new BreadcrumbItem("Don Challenge", href: WebUiEra.UserRoute(Baid, CurrentEra, "DonChallenge"), disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
    }

    private string FormatWindow(DateTimeOffset? startsAt, DateTimeOffset? endsAt)
    {
        return (startsAt, endsAt) switch
        {
            (null, null) => "Always active",
            ({ } start, null) => $"From {start.LocalDateTime.ToString("g", CultureInfo.CurrentCulture)}",
            (null, { } end) => $"Until {end.LocalDateTime.ToString("g", CultureInfo.CurrentCulture)}",
            ({ } start, { } end) =>
                $"{start.LocalDateTime.ToString("g", CultureInfo.CurrentCulture)} - {end.LocalDateTime.ToString("g", CultureInfo.CurrentCulture)}"
        };
    }

    private static Color GetTaskColor(DonChallengeTask task)
        => task.Completed ? Color.Success : Color.Info;

    private static string GetTaskIcon(DonChallengeTask task)
        => task.Completed ? Icons.Material.Filled.CheckCircle : Icons.Material.Filled.RadioButtonUnchecked;

    private static string GetTaskStatus(DonChallengeTask task)
        => task.Completed ? "Completed" : "In progress";

    private static double GetProgressMax(DonChallengeTask task)
        => Math.Max(task.TargetValue ?? task.ProgressValue, 1u);

    private static double GetProgressValue(DonChallengeTask task)
        => Math.Min(task.ProgressValue, (uint)GetProgressMax(task));

    private static string GetProgressText(DonChallengeTask task)
        => task.TargetValue is { } target
            ? $"{task.ProgressValue} / {target}"
            : task.ProgressValue > 0
                ? task.ProgressValue.ToString(CultureInfo.InvariantCulture)
                : "No progress";

    private static string GetTaskTimestamp(DonChallengeTask task)
    {
        if (task.CompletedAt is { } completedAt)
        {
            return $"Completed {completedAt.ToString("g", CultureInfo.CurrentCulture)}";
        }

        return task.UpdatedAt is { } updatedAt
            ? $"Updated {updatedAt.ToString("g", CultureInfo.CurrentCulture)}"
            : string.Empty;
    }

    private MusicDetail? GetMusic(uint songNumber)
        => musicDetailDictionary.TryGetValue(songNumber, out var detail) ? detail : null;

    private static string GetMusicName(MusicDetail detail)
        => string.IsNullOrWhiteSpace(detail.SongNameEN) ? detail.SongName : detail.SongNameEN;

    private static string GetMusicArtist(MusicDetail detail)
        => string.IsNullOrWhiteSpace(detail.ArtistNameEN) ? detail.ArtistName : detail.ArtistNameEN;

    private static string GetDifficultyText(uint level)
    {
        if (Enum.IsDefined(typeof(Difficulty), level) && (Difficulty)level is not Difficulty.None)
        {
            return (Difficulty)level is Difficulty.UraOni ? "Ura Oni" : ((Difficulty)level).ToString();
        }

        return $"Level {level}";
    }

    private string GetRewardLabel(DonChallengeReward reward)
    {
        var parts = reward.RewardSongNoes.Select(songNo => $"Song {ResolveSongLabel(songNo)}")
            .Concat(reward.RewardTitleIds.Select(titleId => $"Title {ResolveTitleLabel(titleId)}"))
            .ToList();

        return parts.Count == 0 ? "No configured reward" : string.Join(", ", parts);
    }

    private string ResolveSongLabel(uint songNo)
    {
        if (!musicDetailDictionary.TryGetValue(songNo, out var detail))
        {
            return $"#{songNo}";
        }

        var name = GetMusicName(detail);
        return string.IsNullOrWhiteSpace(name) ? $"#{songNo}" : name;
    }

    private string ResolveTitleLabel(uint titleId)
    {
        if (!titleDictionary.TryGetValue(titleId, out var title))
        {
            return $"#{titleId}";
        }

        var name = string.IsNullOrWhiteSpace(title.TitleNameEN) ? title.TitleName : title.TitleNameEN;
        return string.IsNullOrWhiteSpace(name) ? $"#{titleId}" : name;
    }

    private static Color GetRewardColor(DonChallengeReward reward)
    {
        return reward.Status switch
        {
            DonChallengeRewardStatus.Earned => Color.Success,
            DonChallengeRewardStatus.Locked => Color.Warning,
            _ => Color.Info
        };
    }

    private static string GetRewardIcon(DonChallengeReward reward)
    {
        return reward.Status switch
        {
            DonChallengeRewardStatus.Earned => Icons.Material.Filled.CheckCircle,
            DonChallengeRewardStatus.Locked => Icons.Material.Filled.Lock,
            _ => Icons.Material.Filled.Info
        };
    }
}
