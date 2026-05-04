using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Infrastructure.Identity;

public class JwtTokenService(IOptions<AuthSettings> options) : IJwtTokenService
{
    private readonly AuthSettings authSettings = options.Value;

    public string IssueToken(uint baid, bool isAdmin)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(authSettings.JwtKey ?? throw new InvalidOperationException());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, baid.ToString()),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = authSettings.JwtIssuer,
            Audience = authSettings.JwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public JwtTokenInfo? ExtractTokenInfo(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return null;
        }

        var jwtToken = handler.ReadJwtToken(token);
        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            return null;
        }

        var claimBaid = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var claimRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (claimBaid == null || claimRole == null)
        {
            return null;
        }

        if (!uint.TryParse(claimBaid, out var baid))
        {
            return null;
        }

        var isAdmin = claimRole == "Admin";
        return new JwtTokenInfo(baid, isAdmin);
    }
}
