using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Settings;
using OtpNet;
using SharedProject.Models.Requests;
using TaikoLocalServer.Filters;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IUserDatumService userDatumService, 
    IOptions<AuthSettings> settings) : BaseController<AuthController>
{
    private readonly AuthSettings authSettings = settings.Value;
    
    private string GenerateJwtToken(uint baid, bool isAdmin)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.JwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, baid.ToString()),
            new(ClaimTypes.Role, isAdmin ? "Admin" : "User")
        };

        var token = new JwtSecurityToken(
            issuer: authSettings.JwtIssuer,
            audience: authSettings.JwtAudience,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials,
            claims: claims
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private static string ComputeHash(string inputPassword, string salt)
    {
        var encDataByte = Encoding.UTF8.GetBytes(inputPassword + salt);
        var encodedData = Convert.ToBase64String(encDataByte);
        encDataByte = Encoding.UTF8.GetBytes(encodedData);
        encodedData = Convert.ToBase64String(encDataByte);
        encDataByte = Encoding.UTF8.GetBytes(encodedData);
        encodedData = Convert.ToBase64String(encDataByte);
        encDataByte = Encoding.UTF8.GetBytes(encodedData);
        encodedData = Convert.ToBase64String(encDataByte);
        return encodedData;
    }
    
    private static Totp MakeTotp(uint baid)
    {
        var secretKey = (baid * 765 + 2023).ToString();
        var base32String = Base32Encoding.ToString(Encoding.UTF8.GetBytes(secretKey));
        var base32Bytes = Base32Encoding.ToBytes(base32String);
        return new Totp(base32Bytes, step: 999999999);
    }
    
    private static string CreateSalt()
    {
        //Generate a cryptographic random number.
        var randomNumber = new byte[32];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var salt = Convert.ToBase64String(randomNumber);

        // Return a Base64 string representation of the random number.
        return salt;
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
        
        var card = await authService.GetCardByAccessCode(accessCode);
        if (card == null) 
            return Unauthorized(new { message = "Access Code Not Found" });
         
        var credential = await authService.GetCredentialByBaid(card.Baid);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });
        
        if (credential.Password == "")
            return Unauthorized(new { message = "User Not Registered" });
        
        // Hash the password with the salt
        var hashedPassword = ComputeHash(password, credential.Salt);

        if (credential.Password != hashedPassword) 
            return Unauthorized(new { message = "Invalid Password" });
        
        // Get User information
        var user = await userDatumService.GetFirstUserDatumOrNull(card.Baid);
        if (user == null)
            return Unauthorized(new { message = "User Does Not Exist" });
        
        var authToken = GenerateJwtToken(card.Baid, user.IsAdmin);
        
        // Return the token with key authToken
        return Ok(new { authToken });
    }
    
    [HttpPost("LoginWithToken")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult LoginWithToken()
    {
        var tokenInfo = authService.ExtractTokenInfo(HttpContext);
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
        
        var card = await authService.GetCardByAccessCode(accessCode);
        if (card == null) 
            return Unauthorized(new { message = "Access Code Not Found" });
        
        var credential = await authService.GetCredentialByBaid(card.Baid);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });
        
        if (credential.Password != "")
            return Unauthorized(new { message = "User Already Registered" });

        if (registerWithLastPlayTime)
        {
            var invited = false;
            if (inviteCode != "")
            {
                invited = VerifyOtp(inviteCode, card.Baid);
            }

            if (!invited)
            {
                var user = await userDatumService.GetFirstUserDatumOrNull(card.Baid);
                if (user == null)
                    return Unauthorized(new { message = "User Does Not Exist" });
                
                var diffMinutes = (lastPlayDateTime - user.LastPlayDatetime).Duration().TotalMinutes;
                if (diffMinutes > 5)
                    return Unauthorized(new { message = "Wrong Last Play Time" });
            }
        }
        
        // Hash the password with the salt
        var salt = CreateSalt();
        var hashedPassword = ComputeHash(password, salt);

        var result = await authService.UpdatePassword(card.Baid, hashedPassword, salt);
        return result ? Ok() : Unauthorized( new { message = "Failed to Update Password" });
    }

    [HttpPost("ChangePassword")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.isAdmin)
            {
                var requestBaid = authService.GetCardByAccessCode(changePasswordRequest.AccessCode).Result?.Baid;
                if (requestBaid != tokenInfo.Value.baid)
                {
                    return Forbid();
                }
            }
        }
        
        var accessCode = changePasswordRequest.AccessCode;
        var oldPassword = changePasswordRequest.OldPassword;
        var newPassword = changePasswordRequest.NewPassword;
        
        var card = await authService.GetCardByAccessCode(accessCode);
        if (card == null) 
            return Unauthorized(new { message = "Access Code Not Found" });
        
        var credential = await authService.GetCredentialByBaid(card.Baid);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });
        
        if (credential.Password == "")
            return Unauthorized(new { message = "User Not Registered" });
        
        // Hash the password with the salt
        var hashedOldPassword = ComputeHash(oldPassword, credential.Salt);
        if (credential.Password != hashedOldPassword) 
            return Unauthorized(new { message = "Wrong Old Password" });
        
        // Hash the new password with the salt
        var salt = CreateSalt();
        var hashedNewPassword = ComputeHash(newPassword, salt);
        
        var result = await authService.UpdatePassword(card.Baid, hashedNewPassword, salt);
        return result ? Ok() : Unauthorized( new { message = "Failed to Update Password" });
    }
    
    [HttpPost("ResetPassword")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.isAdmin && resetPasswordRequest.Baid != tokenInfo.Value.baid)
            {
                return Forbid();
            }
        }
        
        var baid = resetPasswordRequest.Baid;
        
        var credential = await authService.GetCredentialByBaid(baid);
        if (credential == null)
            return Unauthorized(new { message = "Credential Not Found" });
        
        var result = await authService.UpdatePassword(baid, "", "");
        return result ? Ok() : Unauthorized( new { message = "Failed to Reset Password" });
    }

    [HttpPost("GenerateOtp")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GenerateOtp(GenerateOtpRequest generateOtpRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.isAdmin)
            {
                return Forbid();
            }
        }
        
        var totp = MakeTotp(generateOtpRequest.Baid);
        return Ok(new { otp = totp.ComputeTotp() });
    }
}
