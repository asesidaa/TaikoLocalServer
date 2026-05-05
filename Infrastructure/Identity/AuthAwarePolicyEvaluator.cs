using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Infrastructure.Identity;

/// <summary>
/// When <see cref="AuthSettings.AuthenticationRequired"/> is false, every policy
/// short-circuits to success so [Authorize] decorations stay on controllers in
/// "local mode" without granting/denying anything.
/// </summary>
public sealed class AuthAwarePolicyEvaluator : IPolicyEvaluator
{
    private readonly PolicyEvaluator inner;
    private readonly bool authRequired;

    public AuthAwarePolicyEvaluator(IAuthorizationService authorization, IOptions<AuthSettings> options)
    {
        inner = new PolicyEvaluator(authorization);
        authRequired = options.Value.AuthenticationRequired;
    }

    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        => inner.AuthenticateAsync(policy, context);

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource)
        => authRequired
            ? inner.AuthorizeAsync(policy, authenticationResult, context, resource)
            : Task.FromResult(PolicyAuthorizationResult.Success());
}
