namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <param name="Mode">Local (no accounts) or Server.</param>
/// <param name="AuthenticationRequired">Server mode: login is required.</param>
/// <param name="SetupPending">Server mode with no admin yet: Register accepts the setup code from the console.</param>
/// <param name="AllowCustomTitles">Players may type their own title (Player Settings); admins always can.</param>
public sealed record ClientAuthConfigResponse(
    ServerMode Mode,
    bool AuthenticationRequired,
    bool OnlyAdmin,
    int BoundAccessCodeUpperLimit,
    SignUpMode SignUpMode,
    bool AllowUserDelete,
    bool AllowFreeProfileEditing,
    bool SetupPending,
    TwoFactorMode TwoFactor,
    bool AllowCustomTitles = false,
    /// <summary>The games this server runs (ServerSettings.Eras), e.g. Nijiiro, Green.</summary>
    IReadOnlyList<string>? EnabledEras = null,
    /// <summary>Older games' favourite-song caps, by game.</summary>
    IReadOnlyDictionary<string, int>? FavoriteSongLimits = null);
