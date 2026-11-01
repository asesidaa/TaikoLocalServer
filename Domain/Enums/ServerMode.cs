namespace TaikoLocalServer.Domain.Enums;

/// <summary>How the server is run, set by <c>ServerSettings.Mode</c>.</summary>
public enum ServerMode
{
    /// <summary>No accounts or login: everyone on the network is effectively the operator.</summary>
    Local = 0,

    /// <summary>Accounts, login, the admin role and the features that need them.</summary>
    Server = 1,
}
