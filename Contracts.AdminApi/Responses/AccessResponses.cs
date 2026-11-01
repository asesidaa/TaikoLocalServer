namespace TaikoLocalServer.Contracts.AdminApi.Responses;

// Admin › Access: Sign-ins, Sessions, Banned cards. Access codes are always masked here.

public record SignInItem(long Id, DateTime Time, uint? Baid, string? Name, string AccessCode, SignInOutcome Result, string Ip, string UserAgent);

public record SignInPage(IReadOnlyList<SignInItem> Items, long? NextBefore);

public record LockedAccount(uint Baid, string Name, DateTime Until);

public record SignInSummary(int FailedLastDay, IReadOnlyList<LockedAccount> Locked);

public record SessionItem(Guid Id, uint Baid, string Name, DateTime CreatedAt, DateTime LastSeenAt, string Ip, string UserAgent, bool IsCurrent);

public record BanItem(string AccessCode, string MaskedCode, uint? Baid, string? Name, string Reason, uint? BannedBy, string? BannedByName,
    DateTime BannedAt, DateTime? Until, int RefusedCount);
