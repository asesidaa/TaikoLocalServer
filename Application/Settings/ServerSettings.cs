namespace TaikoLocalServer.Application.Settings;

public class ServerSettings
{
    /// <summary>
    /// Local (no accounts or login) or Server (accounts, login, admin role). Unset on installs from before the
    /// switch existed: those count as Server when their AuthSettings.json had AuthenticationRequired on.
    /// </summary>
    public ServerMode? Mode { get; set; }

    /// <summary>The game install extract reads from (the folder containing Data/x64); enables "Run extract".</summary>
    public string? GamePath { get; set; }

    /// <summary>
    /// Let admins send the game files from their browser (Server Management › Game data: pick the game folder) when
    /// the game isn't on this machine. Off: only GamePath, or extract run by hand.
    /// </summary>
    public bool AllowRemoteGameDir { get; set; }

    /// <summary>Allow admins' API tokens (Access › API tokens). Off: tls_ bearers are refused.</summary>
    public bool ApiTokens { get; set; }

    /// <summary>Only cabinets an admin allowed may connect (Server Management › Cabinets). Off: any, except blocked ones.</summary>
    public bool RestrictChassisIds { get; set; }

    public bool EnableMoreSongs { get; set; }

    public int MoreSongsSize { get; set; } = TaikoLocalServer.Domain.DomainConstants.MusicIdMaxExpanded;
}
