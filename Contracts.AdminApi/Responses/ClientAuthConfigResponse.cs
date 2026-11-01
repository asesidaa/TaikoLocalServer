namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <param name="Mode">Local (no accounts) or Server.</param>
/// <param name="AuthenticationRequired">Server mode: login is required.</param>
/// <param name="SetupPending">Server mode with no admin yet: Register accepts the setup code from the console.</param>
public sealed record ClientAuthConfigResponse(
    ServerMode Mode,
    bool AuthenticationRequired,
    bool OnlyAdmin,
    int BoundAccessCodeUpperLimit,
    SignUpMode SignUpMode,
    bool AllowUserDelete,
    bool AllowFreeProfileEditing,
    bool SetupPending,
    TwoFactorMode TwoFactor);
