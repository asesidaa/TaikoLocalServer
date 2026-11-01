namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>An API token as Access › API tokens lists it: never the secret, only its prefix.</summary>
public record ApiTokenItem(int Id, string Name, string Prefix, uint CreatedByBaid, string? CreatedByName, IReadOnlyList<string> Scopes,
    DateTime CreatedAt, DateTime? ExpiresAt, DateTime? LastUsedAt, string? LastUsedIp, DateTime? RevokedAt);

/// <summary>The list, and whether tokens are turned on (ServerSettings.ApiTokens).</summary>
public record ApiTokenList(bool Enabled, IReadOnlyList<ApiTokenItem> Tokens);

/// <summary>A new token: <see cref="Secret"/> is shown this once and never again.</summary>
public record ApiTokenCreated(ApiTokenItem Token, string Secret);

public record CreateApiTokenRequest(string Name, IReadOnlyList<string> Scopes, int? Days);
