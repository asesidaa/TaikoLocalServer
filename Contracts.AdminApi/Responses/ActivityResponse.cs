namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>A page of Access › Activity, newest first. <see cref="NextBefore"/> fetches the next (older) page; null at the end.</summary>
public record ActivityResponse(IReadOnlyList<ActivityItem> Items, long? NextBefore);

public record ActivityItem(long Id, DateTime Time, uint? ActorBaid, string? ActorName, string Action, uint? TargetBaid, string? TargetName, string Detail);
