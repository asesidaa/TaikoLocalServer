using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;
using TaikoLocalServer.Application.Handlers;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[OpenInEveryServerState]
public class AuthController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IServerAccess access,
    IAvatarRenderer avatars,
    IMemoryCache pending) : BaseAdminController<AuthController>
{
    // Login step 1 to step 2: who passed the password, kept briefly, with a few code tries.
    private sealed record PendingSignIn(uint Baid, string AccessCode, bool IsAdmin) { public int Tries; }
    private static readonly TimeSpan PendingLifetime = TimeSpan.FromMinutes(5);
    private const int PendingTries = 5;

    private const int OtpStepSeconds = 3600;
    // 24 prior 1-hour windows = 24h backwards-acceptance for invite codes.
    private static readonly VerificationWindow OtpWindow = new(previous: 24, future: 0);

    private static string ComputeHash(string inputPassword, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(inputPassword, salt);
    }

    private static string CreateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt(10);
    }

    private Totp MakeTotp(uint baid)
    {
        var secret = HMACSHA256.HashData(access.SigningKey, Encoding.UTF8.GetBytes(baid.ToString()));
        return new Totp(secret, step: OtpStepSeconds);
    }

    private bool VerifyOtp(string otp, uint baid)
    {
        var totp = MakeTotp(baid);
        return totp.VerifyTotp(otp, out _, OtpWindow);
    }

    [HttpGet("Config")]
    [AllowAnonymous]
    public ActionResult<ClientAuthConfigResponse> GetConfig()
    {
        // Every WebUI load starts here: while setup is pending, repeat the code at the bottom of the console.
        access.LogSetupCode();
        var rules = access.Rules;
        return Ok(new ClientAuthConfigResponse(access.Mode, access.LoginRequired, rules.OnlyAdmin, rules.AccessCodeLimit,
            rules.SignUpMode, rules.AllowUserDelete, rules.AllowFreeProfileEditing, access.SetupPending, rules.TwoFactor,
            avatars.Status is { Enabled: true, Browser: not null },
            avatars.Status is { Enabled: true, Browser: not null, Cards3D: false }, rules.AllowCustomTitles));
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        // One message for unknown card / wrong password so login can't be used to probe access codes.
        var invalid = Unauthorized(new { message = "Invalid Credentials" });
        var ct = HttpContext.RequestAborted;

        var card = await context.Cards.FindAsync(new object?[] { loginRequest.AccessCode }, ct);
        if (card == null)
            return await Attempt(null, loginRequest.AccessCode, SignInOutcome.UnknownAccessCode, invalid);

        var now = DateTime.UtcNow;
        var ban = await context.BannedCards.FindAsync(new object?[] { card.AccessCode }, ct);
        if (ban is not null && (ban.Until is null || ban.Until > now))
        {
            ban.RefusedCount++;
            return await Attempt(card.Baid, card.AccessCode, SignInOutcome.CardBanned, Unauthorized(new { message = "Card Banned" }));
        }

        if (await IsLockedAsync(card.Baid, now))
            return await Attempt(card.Baid, card.AccessCode, SignInOutcome.AccountLocked, Unauthorized(new { message = "Account Locked" }));

        var credential = await context.Credentials.FindAsync(new object?[] { card.Baid }, ct);
        if (credential == null)
            return await Attempt(card.Baid, card.AccessCode, SignInOutcome.UnknownAccessCode, invalid);

        if (credential.Password == "")
            return await Attempt(card.Baid, card.AccessCode, SignInOutcome.NotRegistered, Unauthorized(new { message = "User Not Registered" }));

        if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, credential.Password))
            return await Attempt(card.Baid, card.AccessCode, SignInOutcome.WrongPassword, invalid);

        var user = await context.UserData
            .FirstOrDefaultAsync(d => d.Baid == card.Baid, ct);
        if (user == null)
            return await Attempt(card.Baid, card.AccessCode, SignInOutcome.UnknownAccessCode, invalid);

        var mode = access.Rules.TwoFactor;
        if (mode != TwoFactorMode.Off && credential.TotpSecret is not null && !await IsTrustedAsync(card.Baid, loginRequest.TrustToken, now))
        {
            // Step 2 next: nothing is signed in until the code checks out. The last 4 digits name the card on that step.
            var pendingToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            pending.Set(PendingKey(pendingToken), new PendingSignIn(card.Baid, card.AccessCode, user.IsAdmin), PendingLifetime);
            return Ok(new { twoFactor = true, pendingToken, last4 = card.AccessCode[^4..] });
        }

        var setupRequired = mode == TwoFactorMode.RequiredForAdmins && user.IsAdmin && credential.TotpSecret is null;
        return await SignIn(card.Baid, card.AccessCode, user.IsAdmin, setupRequired);
    }

    /// <summary>Login step 2: the step-1 token and a code from the authenticator app (or a recovery code).</summary>
    [HttpPost("Verify2fa")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorRequest request)
    {
        if (!pending.TryGetValue(PendingKey(request.PendingToken), out PendingSignIn? p) || p is null)
            return Unauthorized(new { message = "Sign In Expired" });
        if (await IsLockedAsync(p.Baid, DateTime.UtcNow))
        {
            pending.Remove(PendingKey(request.PendingToken));
            return await Attempt(p.Baid, p.AccessCode, SignInOutcome.AccountLocked, Unauthorized(new { message = "Account Locked" }));
        }

        var credential = await context.Credentials.FindAsync(new object?[] { p.Baid }, HttpContext.RequestAborted);
        if (credential is null || !TwoFactorCodes.Verify(credential, request.Code))
        {
            if (Interlocked.Increment(ref p.Tries) >= PendingTries) pending.Remove(PendingKey(request.PendingToken));
            return await Attempt(p.Baid, p.AccessCode, SignInOutcome.WrongTwoFactorCode, Unauthorized(new { message = "Wrong Code" }));
        }
        pending.Remove(PendingKey(request.PendingToken));

        string? trustToken = null;
        if (request.TrustBrowser)
        {
            trustToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            context.TrustedBrowsers.Add(new TrustedBrowser
            {
                Baid = p.Baid, TokenHash = TwoFactorCodes.HashToken(trustToken), CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(TrustLifetime), UserAgent = UserAgent,
            });
        }
        return await SignIn(p.Baid, p.AccessCode, p.IsAdmin, false, trustToken);
    }

    private static readonly TimeSpan TrustLifetime = TimeSpan.FromDays(30);

    private async Task<bool> IsTrustedAsync(uint baid, string? token, DateTime now)
    {
        if (string.IsNullOrEmpty(token) || token.Length > 128) return false;
        var hash = TwoFactorCodes.HashToken(token);
        return await context.TrustedBrowsers.AnyAsync(t => t.Baid == baid && t.TokenHash == hash && t.ExpiresAt > now, HttpContext.RequestAborted);
    }

    private static string PendingKey(string token) => "2fa-pending:" + token;

    private async Task<IActionResult> SignIn(uint baid, string accessCode, bool isAdmin, bool twoFactorSetupRequired, string? trustToken = null)
    {
        var now = DateTime.UtcNow;
        var session = new WebSession
        {
            Id = Guid.NewGuid(), Baid = baid, CreatedAt = now, LastSeenAt = now,
            Ip = Ip, UserAgent = UserAgent,
        };
        context.WebSessions.Add(session);
        var authToken = jwtTokens.IssueToken(baid, isAdmin, session.Id, twoFactorSetupRequired);
        return await Attempt(baid, accessCode, SignInOutcome.SignedIn, trustToken is null ? Ok(new { authToken }) : Ok(new { authToken, trustToken }));
    }

    private string Ip => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
    private string UserAgent => Request.Headers.UserAgent.ToString() is { Length: > 200 } ua ? ua[..200] : Request.Headers.UserAgent.ToString();

    // Logs the attempt for Access › Sign-ins (and the lockout count), then answers with result.
    private async Task<IActionResult> Attempt(uint? baid, string accessCode, SignInOutcome outcome, IActionResult result)
    {
        context.SignInAttempts.Add(new SignInAttempt
        {
            Time = DateTime.UtcNow, Baid = baid, AccessCode = ActivityExtensions.MaskAccessCode(accessCode),
            Result = outcome, Ip = Ip, UserAgent = UserAgent,
        });
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return result;
    }

    /// <summary>
    /// Locked: LockoutAttempts wrong passwords within the last LockoutMinutes, none of them before the last
    /// successful sign-in or an admin's unlock.
    /// </summary>
    private async Task<bool> IsLockedAsync(uint baid, DateTime now)
    {
        var rules = access.Rules;
        var since = now.AddMinutes(-rules.LockoutMinutes);
        var reset = await context.SignInAttempts
            .Where(a => a.Baid == baid && (a.Result == SignInOutcome.SignedIn || a.Result == SignInOutcome.Unlocked))
            .MaxAsync(a => (DateTime?)a.Time, HttpContext.RequestAborted);
        if (reset > since) since = reset.Value;
        var failures = await context.SignInAttempts
            .CountAsync(a => a.Baid == baid && (a.Result == SignInOutcome.WrongPassword || a.Result == SignInOutcome.WrongTwoFactorCode) && a.Time > since, HttpContext.RequestAborted);
        return failures >= rules.LockoutAttempts;
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        // A setup code (Server mode, no admin yet) is the operator's proof: it skips the sign-up rules and makes
        // this account the admin. It's checked last, so a failed registration doesn't use it up.
        var withSetupCode = access.LoginRequired && access.SetupPending && !string.IsNullOrWhiteSpace(registerRequest.SetupCode);
        if (access.LoginRequired && access.Rules.OnlyAdmin && !User.IsAdmin() && !withSetupCode)
            return Forbid();

        var accessCode = registerRequest.AccessCode;
        var password = registerRequest.Password;
        var lastPlayDateTime = registerRequest.LastPlayDateTime;
        var signUp = withSetupCode ? SignUpMode.Open : access.Rules.SignUpMode;
        if (signUp == SignUpMode.Closed && !User.IsAdmin())
            return BadRequest(new { message = "Sign-up Closed" });
        var inviteCode = registerRequest.InviteCode;

        var card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
        var setupCodeUsed = false;
        if (card == null)
        {
            // The first admin's card doesn't have to have played here: create its user, as the game would on
            // its first scan. Only with a valid setup code, checked before anything is created.
            if (!withSetupCode)
                return BadRequest(new { message = "Access Code Not Found" });
            if (accessCode.Length != 20 || !accessCode.All(char.IsAsciiDigit))
                return BadRequest(new { message = "Invalid Access Code" });
            if (password.Length <= 0)
                return BadRequest(new { message = "Password Cannot Be Empty !" });
            if (!access.TryUseSetupCode(registerRequest.SetupCode))
                return BadRequest(new { message = "Wrong Setup Code" });
            setupCodeUsed = true;
            await Mediator.Send(new AddMyDonEntryCommand(accessCode, "Admin", 0), HttpContext.RequestAborted);
            card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
            if (card == null)
                return BadRequest(new { message = "Access Code Not Found" });
        }

        var credential = await context.Credentials.FindAsync(new object?[] { card.Baid }, HttpContext.RequestAborted);
        if (credential == null)
            return BadRequest(new { message = "Credential Not Found" });

        // With a setup code the operator may also pick a card that already has an account: it becomes the admin
        // and gets the new password (the setup code is the stronger proof).
        if (credential.Password != "" && !withSetupCode)
            return BadRequest(new { message = "User Already Registered" });

        if (password.Length <= 0)
            return BadRequest(new { message = "Password Cannot Be Empty !" });

        // Admins setting an account up skip the proof; everyone else proves the card as the sign-up mode says.
        var invited = inviteCode != "" && VerifyOtp(inviteCode, card.Baid);
        if (signUp == SignUpMode.InviteOnly && !invited && !User.IsAdmin())
            return BadRequest(new { message = "Invite Code Required" });
        if (signUp == SignUpMode.LastPlayTime && !User.IsAdmin())
        {
            if (!invited)
            {
                var user = await context.UserData
                    .FirstOrDefaultAsync(d => d.Baid == card.Baid, HttpContext.RequestAborted);
                if (user == null)
                    return BadRequest(new { message = "User Does Not Exist" });

                var diffMinutes = (lastPlayDateTime - user.LastPlayDatetime).Duration().TotalMinutes;
                if (diffMinutes > 5)
                    return BadRequest(new { message = "Wrong Last Play Time" });
            }
        }

        if (withSetupCode)
        {
            if (!setupCodeUsed && !access.TryUseSetupCode(registerRequest.SetupCode))
                return BadRequest(new { message = "Wrong Setup Code" });
            var admin = await context.UserData.FirstOrDefaultAsync(d => d.Baid == card.Baid, HttpContext.RequestAborted);
            if (admin is null)
                return BadRequest(new { message = "User Does Not Exist" });
            admin.IsAdmin = true;
        }

        var salt = CreateSalt();
        var hashedPassword = ComputeHash(password, salt);

        credential.Password = hashedPassword;
        credential.Salt = salt;
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPost("ChangePassword")]
    [Authorize]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        if (access.LoginRequired && access.Rules.OnlyAdmin && !User.IsAdmin())
            return Forbid();

        // The account comes from the token, never from the request body.
        if (User.GetBaid() is not { } baid)
            return BadRequest(new { message = "User Not Registered" });

        var credential = await context.Credentials.FindAsync(new object?[] { baid }, HttpContext.RequestAborted);
        if (credential == null || credential.Password == "")
            return BadRequest(new { message = "User Not Registered" });

        if (!BCrypt.Net.BCrypt.Verify(changePasswordRequest.OldPassword, credential.Password))
            return BadRequest(new { message = "Wrong Old Password" });

        if (changePasswordRequest.NewPassword.Length <= 0)
            return BadRequest(new { message = "Password Cannot Be Empty !" });

        var salt = CreateSalt();
        credential.Password = ComputeHash(changePasswordRequest.NewPassword, salt);
        credential.Salt = salt;
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok();
    }

    /// <summary>
    /// A player deletes their own account (Settings), when the access rules allow it: with their password, and their
    /// 2-step code if it's on ("Code Required" when it's missing). Admins delete accounts in User Management.
    /// </summary>
    [HttpPost("DeleteAccount")]
    [Authorize]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
    {
        if (!access.LoginRequired || !access.Rules.AllowUserDelete || User.IsAdmin())
            return Forbid();
        // The account comes from the token, never from the request body.
        if (User.GetBaid() is not { } baid)
            return BadRequest(new { message = "User Not Registered" });

        var credential = await context.Credentials.FindAsync([baid], HttpContext.RequestAborted);
        if (credential is null || credential.Password == "")
            return BadRequest(new { message = "User Not Registered" });
        if (!BCrypt.Net.BCrypt.Verify(request.Password, credential.Password))
            return BadRequest(new { message = "Wrong Password" });
        if (credential.TotpSecret is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Code)) return BadRequest(new { message = "Code Required" });
            if (!TwoFactorCodes.Verify(credential, request.Code)) return BadRequest(new { message = "Wrong Code" });
        }

        var user = await context.UserData.FindAsync([baid], HttpContext.RequestAborted);
        if (user is null)
            return BadRequest(new { message = "User Not Registered" });
        UserRemoval.Remove(context, user);
        this.LogActivity(context, "user.delete", baid, "their own account");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost("ResetPassword")]
    [ApiScope(write: ApiScopes.AdminUsers)]
    [Authorize]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
    {
        if (this.AuthorizeOwnerOrAdmin(resetPasswordRequest.Baid) is { } forbid)
            return forbid;

        var credential = await context.Credentials.FindAsync(new object?[] { resetPasswordRequest.Baid }, HttpContext.RequestAborted);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });

        credential.Password = "";
        credential.Salt = "";
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPost("GenerateOtp")]
    [ApiScope(write: ApiScopes.AdminUsers)]
    [Authorize(Policy = AuthPolicies.Admin)]
    public IActionResult GenerateOtp(GenerateOtpRequest generateOtpRequest)
    {
        var totp = MakeTotp(generateOtpRequest.Baid);
        return Ok(new { otp = totp.ComputeTotp() });
    }
}
