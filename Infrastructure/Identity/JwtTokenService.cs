using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Infrastructure.Identity;

public class JwtTokenService(IOptions<AuthSettings> options) : IJwtTokenService
{
    private readonly AuthSettings authSettings = options.Value;

    public string IssueToken(uint baid, bool isAdmin)
    {
        var tokenHandler = new JwtSecurityTokenHandler { MapInboundClaims = true };
        var key = Encoding.UTF8.GetBytes(authSettings.JwtKey ?? throw new InvalidOperationException());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, baid.ToString()),
                new Claim(ClaimTypes.Role, isAdmin ? AuthPolicies.Admin : "User")
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = authSettings.JwtIssuer,
            Audience = authSettings.JwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
