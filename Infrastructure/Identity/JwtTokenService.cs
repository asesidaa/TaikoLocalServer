using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Contracts.AdminApi.Authorization;

namespace TaikoLocalServer.Infrastructure.Identity;

public class JwtTokenService(IServerAccess access) : IJwtTokenService
{
    /// <summary>Issuer and audience of every token; fixed, so operators have nothing to configure.</summary>
    public const string Issuer = "TaikoLocalServer";

    public string IssueToken(uint baid, bool isAdmin)
    {
        var tokenHandler = new JwtSecurityTokenHandler { MapInboundClaims = true };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, baid.ToString()),
                new Claim(ClaimTypes.Role, isAdmin ? AuthPolicies.Admin : "User")
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = Issuer,
            Audience = Issuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(access.SigningKey), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
