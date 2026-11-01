using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Contracts.AdminApi.Authorization;

namespace TaikoLocalServer.Infrastructure.Identity;

/// <summary>
/// "Bearer tls_…": an admin's API token. Valid while it isn't revoked or expired and its creator is still an admin;
/// the principal is that admin, with the token's scopes (enforced per endpoint by the admin API's scope filter).
/// </summary>
public sealed class ApiTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    ITaikoDbContext db, IOptions<ServerSettings> settings)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Scheme = "ApiToken";

    // LastUsedAt is written at most this often.
    private static readonly TimeSpan UsedGranularity = TimeSpan.FromMinutes(1);

    public static bool IsApiToken(string? authorization) =>
        authorization?.StartsWith($"Bearer {ApiScopes.Prefix}", StringComparison.Ordinal) == true;

    public static string Hash(string secret) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? header = Request.Headers.Authorization;
        if (!IsApiToken(header)) return AuthenticateResult.NoResult();
        if (!settings.Value.ApiTokens) return AuthenticateResult.Fail("API tokens are turned off (ServerSettings.ApiTokens).");

        var hash = Hash(header!["Bearer ".Length..].Trim());
        var token = await db.ApiTokens.FirstOrDefaultAsync(t => t.Hash == hash, Context.RequestAborted);
        var now = DateTime.UtcNow;
        if (token is null || token.RevokedAt is not null || token.ExpiresAt <= now)
            return AuthenticateResult.Fail("Unknown, revoked or expired token.");
        if (!await db.UserData.AnyAsync(u => u.Baid == token.CreatedByBaid && u.IsAdmin, Context.RequestAborted))
            return AuthenticateResult.Fail("The token's admin is no longer an admin.");

        if (token.LastUsedAt is null || now - token.LastUsedAt > UsedGranularity)
        {
            token.LastUsedAt = now;
            token.LastUsedIp = Context.Connection.RemoteIpAddress?.ToString();
            await db.SaveChangesAsync(Context.RequestAborted);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, token.CreatedByBaid.ToString()),
            new(ClaimTypes.Role, AuthPolicies.Admin),
            new(ApiScopes.TokenClaim, token.Id.ToString()),
            new("api_token_name", token.Name),
        };
        claims.AddRange(token.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => new Claim(ApiScopes.Claim, s)));
        var identity = new ClaimsIdentity(claims, Scheme, ClaimTypes.Name, ClaimTypes.Role);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme));
    }
}
