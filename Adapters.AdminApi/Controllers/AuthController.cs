using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using OtpNet;
using TaikoLocalServer.Application.Handlers;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IServerAccess access) : BaseAdminController<AuthController>
{

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
            rules.RegisterWithLastPlayTime, rules.AllowUserDelete, rules.AllowFreeProfileEditing, access.SetupPending));
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        // One message for unknown card / wrong password so login can't be used to probe access codes.
        var invalid = Unauthorized(new { message = "Invalid Credentials" });

        var card = await context.Cards.FindAsync(new object?[] { loginRequest.AccessCode }, HttpContext.RequestAborted);
        if (card == null)
            return invalid;

        var credential = await context.Credentials.FindAsync(new object?[] { card.Baid }, HttpContext.RequestAborted);
        if (credential == null)
            return invalid;

        if (credential.Password == "")
            return Unauthorized(new { message = "User Not Registered" });

        if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, credential.Password))
            return invalid;

        var user = await context.UserData
            .FirstOrDefaultAsync(d => d.Baid == card.Baid, HttpContext.RequestAborted);
        if (user == null)
            return invalid;

        var authToken = jwtTokens.IssueToken(card.Baid, user.IsAdmin);

        return Ok(new { authToken });
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
        var registerWithLastPlayTime = access.Rules.RegisterWithLastPlayTime && !withSetupCode;
        var inviteCode = registerRequest.InviteCode;

        var card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
        var setupCodeUsed = false;
        if (card == null)
        {
            // The first admin's card doesn't have to have played here: create its user, as the game would on
            // its first scan. Only with a valid setup code, checked before anything is created.
            if (!withSetupCode)
                return Unauthorized(new { message = "Access Code Not Found" });
            if (accessCode.Length != 20 || !accessCode.All(char.IsAsciiDigit))
                return BadRequest(new { message = "Invalid Access Code" });
            if (password.Length <= 0)
                return Unauthorized(new { message = "Password Cannot Be Empty !" });
            if (!access.TryUseSetupCode(registerRequest.SetupCode))
                return Unauthorized(new { message = "Wrong Setup Code" });
            setupCodeUsed = true;
            await Mediator.Send(new AddMyDonEntryCommand(accessCode, "Admin", 0), HttpContext.RequestAborted);
            card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
            if (card == null)
                return Unauthorized(new { message = "Access Code Not Found" });
        }

        var credential = await context.Credentials.FindAsync(new object?[] { card.Baid }, HttpContext.RequestAborted);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });

        // With a setup code the operator may also pick a card that already has an account: it becomes the admin
        // and gets the new password (the setup code is the stronger proof).
        if (credential.Password != "" && !withSetupCode)
            return Unauthorized(new { message = "User Already Registered" });

        if (password.Length <= 0)
            return Unauthorized(new { message = "Password Cannot Be Empty !" });

        if (registerWithLastPlayTime)
        {
            var invited = false;
            if (inviteCode != "")
            {
                invited = VerifyOtp(inviteCode, card.Baid);
            }

            if (!invited)
            {
                var user = await context.UserData
                    .FirstOrDefaultAsync(d => d.Baid == card.Baid, HttpContext.RequestAborted);
                if (user == null)
                    return Unauthorized(new { message = "User Does Not Exist" });

                var diffMinutes = (lastPlayDateTime - user.LastPlayDatetime).Duration().TotalMinutes;
                if (diffMinutes > 5)
                    return Unauthorized(new { message = "Wrong Last Play Time" });
            }
        }

        if (withSetupCode)
        {
            if (!setupCodeUsed && !access.TryUseSetupCode(registerRequest.SetupCode))
                return Unauthorized(new { message = "Wrong Setup Code" });
            var admin = await context.UserData.FirstOrDefaultAsync(d => d.Baid == card.Baid, HttpContext.RequestAborted);
            if (admin is null)
                return Unauthorized(new { message = "User Does Not Exist" });
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
            return Unauthorized(new { message = "User Not Registered" });

        var credential = await context.Credentials.FindAsync(new object?[] { baid }, HttpContext.RequestAborted);
        if (credential == null || credential.Password == "")
            return Unauthorized(new { message = "User Not Registered" });

        if (!BCrypt.Net.BCrypt.Verify(changePasswordRequest.OldPassword, credential.Password))
            return Unauthorized(new { message = "Wrong Old Password" });

        if (changePasswordRequest.NewPassword.Length <= 0)
            return Unauthorized(new { message = "Password Cannot Be Empty !" });

        var salt = CreateSalt();
        credential.Password = ComputeHash(changePasswordRequest.NewPassword, salt);
        credential.Salt = salt;
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPost("ResetPassword")]
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
    [Authorize(Policy = AuthPolicies.Admin)]
    public IActionResult GenerateOtp(GenerateOtpRequest generateOtpRequest)
    {
        var totp = MakeTotp(generateOtpRequest.Baid);
        return Ok(new { otp = totp.ComputeTotp() });
    }
}
