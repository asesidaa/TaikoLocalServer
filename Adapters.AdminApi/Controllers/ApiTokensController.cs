using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure.Identity;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Access › API tokens: admins' tokens for bots, leaderboard sites and scripts. Managed from a Web UI sign-in only
/// (no [ApiScope], so a token can't make or revoke tokens).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public class ApiTokensController(ITaikoDbContext context, IOptions<ServerSettings> settings) : BaseAdminController<ApiTokensController>
{
    private const string Alphabet = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    [HttpGet]
    public async Task<ApiTokenList> Get()
    {
        var tokens = await context.ApiTokens.OrderByDescending(t => t.CreatedAt).ToListAsync(HttpContext.RequestAborted);
        var ids = tokens.Select(t => t.CreatedByBaid).Distinct().ToList();
        var names = await context.UserData.Where(u => ids.Contains(u.Baid)).ToDictionaryAsync(u => u.Baid, u => u.MyDonName, HttpContext.RequestAborted);
        return new ApiTokenList(settings.Value.ApiTokens, tokens.Select(t => Item(t, names.GetValueOrDefault(t.CreatedByBaid))).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ApiTokenCreated>> Create(CreateApiTokenRequest request)
    {
        var name = request.Name?.Trim() ?? "";
        if (name.Length is 0 or > 60) return BadRequest(new { message = "A token needs a name (up to 60 characters)." });
        var scopes = request.Scopes.Distinct().ToList();
        if (scopes.Count == 0 || scopes.Any(s => ApiScopes.All.All(x => x.Scope != s)))
            return BadRequest(new { message = "Pick at least one known scope." });
        if (request.Days is <= 0) return BadRequest(new { message = "Expiry must be at least a day." });
        if (User.GetBaid() is not { } baid) return Forbid();

        var secret = ApiScopes.Prefix + RandomNumberGenerator.GetString(Alphabet, 24);
        var now = DateTime.UtcNow;
        var token = new ApiToken
        {
            CreatedByBaid = baid, Name = name, Prefix = secret[..8], Hash = ApiTokenAuthenticationHandler.Hash(secret),
            Scopes = string.Join(' ', scopes), CreatedAt = now, ExpiresAt = request.Days is { } d ? now.AddDays(d) : null,
        };
        context.ApiTokens.Add(token);
        this.LogActivity(context, "token.create", null, $"{name} · {token.Scopes}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        var me = await context.UserData.FindAsync([baid], HttpContext.RequestAborted);
        return Ok(new ApiTokenCreated(Item(token, me?.MyDonName), secret));
    }

    /// <summary>Revokes a token (it stops working at once); a revoked or expired one is removed from the list.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var token = await context.ApiTokens.FindAsync([id], HttpContext.RequestAborted);
        if (token is null) return NotFound();
        if (token.RevokedAt is null && !(token.ExpiresAt <= DateTime.UtcNow))
        {
            token.RevokedAt = DateTime.UtcNow;
            this.LogActivity(context, "token.revoke", null, token.Name);
        }
        else
        {
            context.ApiTokens.Remove(token);
        }
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static ApiTokenItem Item(ApiToken t, string? by) => new(t.Id, t.Name, t.Prefix, t.CreatedByBaid, by,
        t.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries), Utc(t.CreatedAt), Utc(t.ExpiresAt), Utc(t.LastUsedAt), t.LastUsedIp, Utc(t.RevokedAt));

    private static DateTime Utc(DateTime t) => DateTime.SpecifyKind(t, DateTimeKind.Utc);
    private static DateTime? Utc(DateTime? t) => t is { } v ? Utc(v) : null;
}
