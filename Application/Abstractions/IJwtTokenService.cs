namespace TaikoLocalServer.Application.Abstractions;

public interface IJwtTokenService
{
    /// <summary>A Web UI token for <paramref name="baid"/>, tied to its session (revoking the session ends it).</summary>
    string IssueToken(uint baid, bool isAdmin, Guid sessionId);

    /// <summary>The claim that carries the session id.</summary>
    const string SessionClaim = "sid";
}
