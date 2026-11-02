namespace TaikoLocalServer.Application.Settings;

public sealed class ServerSettings
{
    /// <summary>
    /// Local (no accounts or login) or Server (accounts, login, admin role). Unset on installs from before the
    /// switch existed: those count as Server when their AuthSettings.json had AuthenticationRequired on.
    /// </summary>
    public ServerMode? Mode { get; set; }

    /// <summary>
    /// Let admins send the game files from their browser (Server Management › Game data: pick the game folder) when
    /// the game isn't on this machine. Off: only Eras.Nijiiro.GameDataPath, or extract run by hand.
    /// </summary>
    public bool AllowRemoteGameDir { get; set; }

    /// <summary>Allow admins' API tokens (Access › API tokens). Off: tls_ bearers are refused.</summary>
    public bool ApiTokens { get; set; }

    /// <summary>Only cabinets an admin allowed may connect (Server Management › Cabinets). Off: any, except blocked ones.</summary>
    public bool RestrictChassisIds { get; set; }

    public bool EnableMoreSongs { get; set; }

    public int MoreSongsSize { get; set; } = TaikoLocalServer.Domain.DomainConstants.MusicIdMaxExpanded;

    public Dictionary<string, EraSettings> Eras { get; set; } = new();
}

public sealed class EraSettings
{
    public bool Enabled { get; set; }

    public bool AutoExtractCatalog { get; set; } = true;

    public string GameDataPath { get; set; } = string.Empty;

    /// <summary>
    /// An older game's install (RPCS3, its USRDIR/data, or a .zip), which extract reads when an admin extracts it
    /// (Server Management › Games). Nijiiro's is GameDataPath. Relative paths are from the server's folder.
    /// </summary>
    public string GamePath { get; set; } = string.Empty;

    public string? CustomizationNameDataPath { get; set; }

    public bool? EnableShop { get; set; }

    public uint? ActiveShopSeasonId { get; set; }

    public bool? EnableDonChallenge { get; set; }

    public string? ActiveDonChallengeBundleId { get; set; }

    public bool IsDonChallengeEnabled()
        => EnableDonChallenge.GetValueOrDefault();

    public string? GetActiveDonChallengeBundleId()
        => ActiveDonChallengeBundleId;
}
