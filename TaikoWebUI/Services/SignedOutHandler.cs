namespace TaikoWebUI.Services;

/// <summary>
/// The server answers 401 to a signed-in request when that session was signed out (Access › Sessions, Sign out
/// everywhere), the user is no longer an admin, or the token expired. Then this browser is signed out too. So an
/// input mistake (wrong password, wrong code) must answer 400, never 401.
/// </summary>
public sealed class SignedOutHandler : DelegatingHandler
{
    /// <summary>Raised once per 401 on a request that carried a token; the auth state provider subscribes.</summary>
    public static event Func<Task>? SignedOut;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && request.Headers.Authorization is not null && SignedOut is { } signedOut)
            await signedOut();
        return response;
    }
}
