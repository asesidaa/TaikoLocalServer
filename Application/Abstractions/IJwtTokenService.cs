namespace TaikoLocalServer.Application.Abstractions;

public interface IJwtTokenService
{
    /// <summary>A Web UI token for <paramref name="baid"/>, tied to its session (revoking the session ends it).</summary>
    /// <param name="twoFactorSetupRequired">An admin who must turn on 2-step first: the token is a player's until they do.</param>
    string IssueToken(uint baid, bool isAdmin, Guid sessionId, bool twoFactorSetupRequired = false);

    /// <summary>The claim that carries the session id.</summary>
    const string SessionClaim = "sid";

    /// <summary>Present ("1") on the token of an admin who must turn on 2-step verification.</summary>
    const string TwoFactorSetupClaim = "tfa_setup";
}
