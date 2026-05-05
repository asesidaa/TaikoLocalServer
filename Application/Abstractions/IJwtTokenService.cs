namespace TaikoLocalServer.Application.Abstractions;

public interface IJwtTokenService
{
    string IssueToken(uint baid, bool isAdmin);
}
