using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OtpNet;
using SharedProject.Models.Requests;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings) : BaseAdminController<AuthController>
{
    private readonly AuthSettings authSettings = settings.Value;

    private static string ComputeHash(string inputPassword, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(inputPassword, salt);
    }

    private static string CreateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt(10);
    }

    private static Totp MakeTotp(uint baid)
    {
        var secretKey = (baid * 765 + 2023).ToString();
        var base32String = Base32Encoding.ToString(Encoding.UTF8.GetBytes(secretKey));
        var base32Bytes = Base32Encoding.ToBytes(base32String);
        return new Totp(base32Bytes, step: 999999999);
    }

    private static bool VerifyOtp(string otp, uint baid)
    {
        var totp = MakeTotp(baid);
        return totp.VerifyTotp(otp, out _);
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
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult LoginWithToken()
    {
        var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
        if (tokenInfo == null)
        {
            return Unauthorized();
        }

        return Ok();
    }


    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        var accessCode = registerRequest.AccessCode;
        var password = registerRequest.Password;
        var lastPlayDateTime = registerRequest.LastPlayDateTime;
        var registerWithLastPlayTime = registerRequest.RegisterWithLastPlayTime;
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

                var diffMinutes = (lastPlayDateTime - user.LastPlayDatetime).Duration().TotalMinutes;
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
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.IsAdmin)
            {
                var requestCard = await context.Cards.FindAsync(new object?[] { changePasswordRequest.AccessCode }, HttpContext.RequestAborted);
                if (requestCard?.Baid != tokenInfo.Value.Baid)
                {
                    return Forbid();
                }
            }
        }

        var accessCode = changePasswordRequest.AccessCode;
        var oldPassword = changePasswordRequest.OldPassword;
        var newPassword = changePasswordRequest.NewPassword;

        var card = await context.Cards.FindAsync(new object?[] { accessCode }, HttpContext.RequestAborted);
        if (card == null)
            return Unauthorized(new { message = "Access Code Not Found" });

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
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.IsAdmin && resetPasswordRequest.Baid != tokenInfo.Value.Baid)
            {
                return Forbid();
            }
        }

        var baid = resetPasswordRequest.Baid;

        var credential = await context.Credentials.FindAsync(new object?[] { baid }, HttpContext.RequestAborted);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });

        credential.Password = "";
        credential.Salt = "";
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPost("GenerateOtp")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GenerateOtp(GenerateOtpRequest generateOtpRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.IsAdmin)
            {
                return Forbid();
            }
        }

        var totp = MakeTotp(generateOtpRequest.Baid);
        return Ok(new { otp = totp.ComputeTotp() });
    }
}
