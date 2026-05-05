namespace TaikoWebUI.Authorization;

public static class WebUiAuthPolicies
{
    /// <summary>
    /// Pages tagged with this policy require real authentication (a server-issued bearer token).
    /// In local mode the policy fails so the AuthorizeRouteView's NotAuthorized block fires
    /// and the user is redirected away — there is nothing to log in to.
    /// </summary>
    public const string RequireRealAuth = "RequireRealAuth";
}
