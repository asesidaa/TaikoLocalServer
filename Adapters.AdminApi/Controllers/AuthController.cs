using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OtpNet;
using TaikoLocalServer.Adapters.AdminApi.Mapping;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings,
    IOptions<ServerSettings> serverSettings) : BaseAdminController<AuthController>
{
    private readonly AuthSettings authSettings = settings.Value;
    private readonly ServerSettings serverSettings = serverSettings.Value;

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
        var jwtKey = authSettings.JwtKey;
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JwtKey must be configured to derive OTP secrets.");
        }

        var secret = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(jwtKey),
            Encoding.UTF8.GetBytes(baid.ToString()));
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
        var enabledEras = serverSettings.Eras
            .Where(pair => pair.Value.Enabled)
            .Select(pair => pair.Key)
            .OrderBy(era => era)
            .ToList();

        var response = authSettings.ToResponse() with
        {
            EnabledEras = enabledEras,
            FavoriteSongLimits = GetFavoriteSongLimits(enabledEras)
        };

        return Ok(response);
    }

    private static IReadOnlyDictionary<string, int> GetFavoriteSongLimits(IEnumerable<string> enabledEras)
    {
        var limits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var eraName in enabledEras)
        {
            if (Enum.TryParse<GameEra>(eraName, ignoreCase: true, out var era)
                && Ac15EraProfiles.GetMaxFavoriteSongs(era) is { } limit)
            {
                limits[eraName] = limit;
            }
        }

        return limits;
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var accessCode = loginRequest.AccessCode;
        var password = loginRequest.Password;

        var card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
        if (card == null)
            return Unauthorized(new { message = "Access Code Not Found" });

        var credential = await context.Credentials.FindAsync(new object?[] { card.Baid }, HttpContext.RequestAborted);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });

        if (credential.Password == "")
            return Unauthorized(new { message = "User Not Registered" });

        var hashedPassword = ComputeHash(password, credential.Salt);

        if (credential.Password != hashedPassword)
            return Unauthorized(new { message = "Invalid Password" });

        var user = await context.UserData
            .Include(d => d.Tokens)
            .FirstOrDefaultAsync(d => d.Baid == card.Baid, HttpContext.RequestAborted);
        if (user == null)
            return Unauthorized(new { message = "User Does Not Exist" });

        var authToken = jwtTokens.IssueToken(card.Baid, user.IsAdmin);

        return Ok(new { authToken });
    }

    [HttpPost("LoginWithToken")]
    [Authorize]
    public IActionResult LoginWithToken() => Ok();


    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        if (authSettings.AuthenticationRequired && authSettings.OnlyAdmin && !User.IsAdmin())
            return Forbid();

        var accessCode = registerRequest.AccessCode;
        var password = registerRequest.Password;
        var lastPlayDateTime = registerRequest.LastPlayDateTime;
        var registerWithLastPlayTime = authSettings.RegisterWithLastPlayTime;
        var inviteCode = registerRequest.InviteCode;

        var card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
        if (card == null)
            return Unauthorized(new { message = "Access Code Not Found" });

        var credential = await context.Credentials.FindAsync(new object?[] { card.Baid }, HttpContext.RequestAborted);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });

        if (credential.Password != "")
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
                    .Include(d => d.Tokens)
                    .FirstOrDefaultAsync(d => d.Baid == card.Baid, HttpContext.RequestAborted);
                if (user == null)
                    return Unauthorized(new { message = "User Does Not Exist" });

                var saveData = await context.GetOrCreateNijiiroSaveDataAsync(card.Baid, HttpContext.RequestAborted);
                var diffMinutes = (lastPlayDateTime - saveData.LastPlayDatetime).Duration().TotalMinutes;
                if (diffMinutes > 5)
                    return Unauthorized(new { message = "Wrong Last Play Time" });
            }
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
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        if (authSettings.AuthenticationRequired && authSettings.OnlyAdmin && !User.IsAdmin())
            return Forbid();

        var accessCode = changePasswordRequest.AccessCode;
        var oldPassword = changePasswordRequest.OldPassword;
        var newPassword = changePasswordRequest.NewPassword;

        var card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
        if (card == null)
            return Unauthorized(new { message = "Access Code Not Found" });

        if (this.AuthorizeOwnerOrAdmin(card.Baid) is { } forbid)
            return forbid;

        var credential = await context.Credentials.FindAsync(new object?[] { card.Baid }, HttpContext.RequestAborted);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });

        if (credential.Password == "")
            return Unauthorized(new { message = "User Not Registered" });

        var hashedOldPassword = ComputeHash(oldPassword, credential.Salt);
        if (credential.Password != hashedOldPassword)
            return Unauthorized(new { message = "Wrong Old Password" });

        if (newPassword.Length <= 0)
            return Unauthorized(new { message = "Password Cannot Be Empty !" });

        var salt = CreateSalt();
        var hashedNewPassword = ComputeHash(newPassword, salt);

        credential.Password = hashedNewPassword;
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
