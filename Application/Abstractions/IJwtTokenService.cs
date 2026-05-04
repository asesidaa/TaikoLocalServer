using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Application.Abstractions;

public readonly record struct JwtTokenInfo(uint Baid, bool IsAdmin);

public interface IJwtTokenService
{
    string IssueToken(uint baid, bool isAdmin);

    JwtTokenInfo? ExtractTokenInfo(HttpContext httpContext);
}
