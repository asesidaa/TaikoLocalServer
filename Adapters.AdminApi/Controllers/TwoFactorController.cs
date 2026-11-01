using Microsoft.AspNetCore.RateLimiting;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// The signed-in user's 2-step verification (Settings › Security): set up with a key and a code, then recovery codes.
/// Admins can reset someone's (a lost phone) from User Management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.AdminUsers, ApiScopes.AdminUsers)]
public class TwoFactorController(ITaikoDbContext context, IServerAccess access, IJwtTokenService jwtTokens) : BaseAdminController<TwoFactorController>
{
    [HttpGet]
    public async Task<ActionResult<TwoFactorStatus>> Get()
    {
        var credential = await Mine();
        return new TwoFactorStatus
        {
            Mode = access.Rules.TwoFactor,
            Enabled = credential?.TotpSecret is not null,
            RecoveryCodesLeft = credential is null ? 0 : TwoFactorCodes.RecoveryCodesLeft(credential),
        };
    }

    /// <summary>A new key to put in the authenticator app. Nothing changes until <see cref="Enable"/>.</summary>
    [HttpPost("Setup")]
    public async Task<ActionResult<TwoFactorSetup>> Setup()
    {
        if (access.Rules.TwoFactor == TwoFactorMode.Off) return BadRequest(new { message = "2-step verification is off on this server." });
        var card = await context.Cards.Where(c => c.Baid == User.GetBaid()).Select(c => c.AccessCode).FirstOrDefaultAsync(HttpContext.RequestAborted);
        var secret = TwoFactorCodes.NewSecret();
        var uri = TwoFactorCodes.Uri(secret, card is { Length: >= 4 } ? $"card {card[^4..]}" : $"player {User.GetBaid()}");
        return new TwoFactorSetup { Secret = secret, Uri = uri, QrCode = TwoFactorCodes.QrCode(uri) };
    }

    [HttpPost("Enable")]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<ActionResult<TwoFactorCodesResponse>> Enable(TwoFactorCodeRequest request)
    {
        if (access.Rules.TwoFactor == TwoFactorMode.Off) return BadRequest(new { message = "2-step verification is off on this server." });
        var credential = await Mine();
        if (credential is null) return NotFound();
        if (credential.TotpSecret is not null) return Conflict(new { message = "Already on." });
        if (TwoFactorCodes.MatchTotp(request.Secret, request.Code) is not { } step) return BadRequest(new { message = "Wrong Code" });

        credential.TotpSecret = request.Secret;
        credential.TotpLastStep = step;
        var codes = TwoFactorCodes.NewRecoveryCodes(credential);
        this.LogActivity(context, "2fa.enable", User.GetBaid());

        // An admin who had to turn it on first: this session becomes an admin one now.
        string? token = null;
        if (User.FindFirst(IJwtTokenService.TwoFactorSetupClaim) is not null &&
            Guid.TryParse(User.FindFirst(IJwtTokenService.SessionClaim)?.Value, out var sid) &&
            await context.UserData.AnyAsync(u => u.Baid == credential.Baid && u.IsAdmin, HttpContext.RequestAborted))
            token = jwtTokens.IssueToken(credential.Baid, true, sid);

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return new TwoFactorCodesResponse { RecoveryCodes = codes, AuthToken = token };
    }

    /// <summary>New recovery codes (the old ones stop working); needs a current code.</summary>
    [HttpPost("RecoveryCodes")]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<ActionResult<TwoFactorCodesResponse>> NewRecoveryCodes(TwoFactorCodeRequest request)
    {
        var credential = await Mine();
        if (credential?.TotpSecret is null) return NotFound();
        if (!TwoFactorCodes.Verify(credential, request.Code)) return BadRequest(new { message = "Wrong Code" });
        var codes = TwoFactorCodes.NewRecoveryCodes(credential);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return new TwoFactorCodesResponse { RecoveryCodes = codes };
    }

    /// <summary>Turns it off; needs a current code or a recovery code. Not for admins while it's required for them.</summary>
    [HttpPost("Disable")]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<IActionResult> Disable(TwoFactorCodeRequest request)
    {
        var credential = await Mine();
        if (credential?.TotpSecret is null) return NotFound();
        if (access.Rules.TwoFactor == TwoFactorMode.RequiredForAdmins && User.IsAdmin())
            return BadRequest(new { message = "Required for admins." });
        if (!TwoFactorCodes.Verify(credential, request.Code)) return BadRequest(new { message = "Wrong Code" });
        TwoFactorCodes.Clear(credential);
        await ForgetBrowsers(credential.Baid);
        this.LogActivity(context, "2fa.disable", credential.Baid);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>An admin turns someone's off (they lost their phone and their recovery codes).</summary>
    [HttpDelete("{baid}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> Reset(uint baid)
    {
        var credential = await context.Credentials.FindAsync([baid], HttpContext.RequestAborted);
        if (credential?.TotpSecret is null) return NotFound();
        TwoFactorCodes.Clear(credential);
        await ForgetBrowsers(baid);
        this.LogActivity(context, "2fa.reset", baid);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task ForgetBrowsers(uint baid) =>
        context.TrustedBrowsers.RemoveRange(await context.TrustedBrowsers.Where(t => t.Baid == baid).ToListAsync(HttpContext.RequestAborted));

    private async Task<Credential?> Mine() => await context.Credentials.FindAsync([User.GetBaid()], HttpContext.RequestAborted);
}
