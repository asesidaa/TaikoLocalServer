namespace TaikoLocalServer.Application.Abstractions;

/// <summary>
/// How the server is run and who may do what: the mode (from config), the runtime access rules (from the
/// database, admin-editable) and the server's own signing key. Loaded once at startup.
/// </summary>
public interface IServerAccess
{
    ServerMode Mode { get; }

    /// <summary>Server mode: accounts and login. Local mode has neither, so every check passes.</summary>
    bool LoginRequired => Mode == ServerMode.Server;

    /// <summary>A copy of the current rules (they only matter in Server mode).</summary>
    AccessRules Rules { get; }

    Task UpdateRulesAsync(AccessRules rules, CancellationToken cancellationToken);

    /// <summary>The key that signs login tokens and derives invite codes.</summary>
    byte[] SigningKey { get; }

    /// <summary>Server mode with no admin yet: a one-time setup code was printed in the console at startup.</summary>
    bool SetupPending { get; }

    /// <summary>True (once) when <paramref name="code"/> is the setup code; the caller then makes that account admin.</summary>
    bool TryUseSetupCode(string code);
}
