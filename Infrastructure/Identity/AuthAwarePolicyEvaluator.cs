using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Infrastructure.Identity;

/// <summary>
/// In Local mode (<see cref="IServerAccess.LoginRequired"/> false) every policy
/// short-circuits to success, so [Authorize] decorations stay on controllers
/// without granting or denying anything. While setup is pending, the setup code (the
/// <see cref="SetupCodeHeader"/> header the wizard sends) passes every policy too: there's no admin to sign in yet.
/// </summary>
public sealed class AuthAwarePolicyEvaluator : IPolicyEvaluator
{
    public const string SetupCodeHeader = "X-Setup-Code";

    private readonly PolicyEvaluator inner;
    private readonly IServerAccess access;
    private readonly bool authRequired;

    public AuthAwarePolicyEvaluator(IAuthorizationService authorization, IServerAccess access)
    {
        inner = new PolicyEvaluator(authorization);
        this.access = access;
        authRequired = access.LoginRequired;
    }

    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        => inner.AuthenticateAsync(policy, context);

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource)
        => !authRequired || (access.SetupPending && access.IsSetupCode(context.Request.Headers[SetupCodeHeader]))
            ? Task.FromResult(PolicyAuthorizationResult.Success())
            : inner.AuthorizeAsync(policy, authenticationResult, context, resource);
}
