using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Infrastructure.Identity;

/// <summary>
/// In Local mode (<see cref="IServerAccess.LoginRequired"/> false) every policy
/// short-circuits to success, so [Authorize] decorations stay on controllers
/// without granting or denying anything.
/// </summary>
public sealed class AuthAwarePolicyEvaluator : IPolicyEvaluator
{
    private readonly PolicyEvaluator inner;
    private readonly bool authRequired;

    public AuthAwarePolicyEvaluator(IAuthorizationService authorization, IServerAccess access)
    {
        inner = new PolicyEvaluator(authorization);
        authRequired = access.LoginRequired;
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
