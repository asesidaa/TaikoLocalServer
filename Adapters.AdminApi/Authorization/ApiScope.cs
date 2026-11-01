using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

/// <summary>
/// The scope an API token needs here: <paramref name="read"/> for GET, <paramref name="write"/> for anything else.
/// On a controller or an action (the action wins). Null, or no attribute at all: API tokens are refused.
/// Web UI sign-ins don't need scopes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiScopeAttribute(string? read = null, string? write = null) : Attribute
{
    public string? Read { get; } = read;
    public string? Write { get; } = write;
}

/// <summary>Enforces <see cref="ApiScopeAttribute"/> for API-token principals; registered for every controller.</summary>
public sealed class ApiScopeFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.HasClaim(c => c.Type == ApiScopes.TokenClaim)) return;

        var attribute = context.ActionDescriptor.EndpointMetadata.OfType<ApiScopeAttribute>().LastOrDefault();
        var method = context.HttpContext.Request.Method;
        var needed = HttpMethods.IsGet(method) || HttpMethods.IsHead(method) ? attribute?.Read : attribute?.Write;
        if (needed is null || !user.HasClaim(ApiScopes.Claim, needed))
            context.Result = new ObjectResult(new { message = needed is null ? "API tokens can't use this endpoint." : $"This token needs the {needed} scope." })
                { StatusCode = StatusCodes.Status403Forbidden };
    }
}
