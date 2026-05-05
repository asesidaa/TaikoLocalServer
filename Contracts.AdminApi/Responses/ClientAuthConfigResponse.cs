namespace TaikoLocalServer.Contracts.AdminApi.Responses;

public sealed record ClientAuthConfigResponse(
    bool AuthenticationRequired,
    bool OnlyAdmin,
    int BoundAccessCodeUpperLimit,
    bool RegisterWithLastPlayTime,
    bool AllowUserDelete,
    bool AllowFreeProfileEditing);
