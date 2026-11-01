namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <param name="Mode">Local (no accounts) or Server.</param>
/// <param name="AuthenticationRequired">Server mode: login is required.</param>
/// <param name="SetupPending">Server mode with no admin yet: Register accepts the setup code from the console.</param>
/// <param name="ServerAvatars">The server draws avatars itself (Server Management › Settings), so browsers don't.</param>
/// <param name="Quality3D">The 3D model's pixel density on player cards (Player Settings).</param>
/// <param name="ServerStills">Player cards show still images the server draws instead of the 3D model ("Enable 3D Renderer" off).</param>
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
    bool ServerAvatars,
    bool ServerStills = false,
    RenderQuality Quality3D = RenderQuality.High);
