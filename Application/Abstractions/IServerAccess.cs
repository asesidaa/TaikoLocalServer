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

    /// <summary>A copy of the server state an admin set; <see cref="ServerStatus.At"/> gives the one in force.</summary>
    ServerStatus Status { get; }

    Task UpdateStatusAsync(ServerStatus status, CancellationToken cancellationToken);

    /// <summary>The key that signs login tokens and derives invite codes.</summary>
    byte[] SigningKey { get; }

    /// <summary>
    /// The setup wizard isn't finished (or Server mode has no admin): a setup code was printed in the console at
    /// startup, and it unlocks the admin pages the wizard uses.
    /// </summary>
    bool SetupPending { get; }

    /// <summary>The setup code while setup is pending (carried over a restart the wizard asks for); null after.</summary>
    string? SetupCode { get; }

    /// <summary>Whether <paramref name="code"/> is the setup code. Doesn't use it up.</summary>
    bool IsSetupCode(string? code);

    /// <summary>True (once) when <paramref name="code"/> is the setup code; the caller then makes that account admin.</summary>
    bool TryUseSetupCode(string code);

    /// <summary>The wizard is done: saved in the database, and the setup code stops working.</summary>
    Task FinishSetupAsync(CancellationToken cancellationToken);

    /// <summary>Logs the setup code again (if one is pending), so it's the newest console line when someone opens the WebUI.</summary>
    void LogSetupCode();
}
