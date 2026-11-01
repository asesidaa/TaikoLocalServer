using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

/// <summary>Open whatever the server state: signing in and out, and reading the state itself.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class OpenInEveryServerStateAttribute : Attribute;

/// <summary>
/// The Web UI side of the server state (Server Management › Status), for players in Server mode: Offline closes the
/// admin API to them, Maintenance pauses their saves (anything but GET). Both answer 503 with the state, which the Web
/// UI turns into the offline page or a "saving is paused" toast. Admins, and everyone in Local mode, aren't affected.
/// </summary>
public sealed class ServerStateFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Registered for every controller: the game protocol has its own (CabinetFilter).
        if (context.ActionDescriptor is not ControllerActionDescriptor action || action.ControllerTypeInfo.Assembly != typeof(ServerStateFilter).Assembly)
            return;
        if (action.EndpointMetadata.OfType<OpenInEveryServerStateAttribute>().Any()) return;

        var access = context.HttpContext.RequestServices.GetRequiredService<IServerAccess>();
        if (!access.LoginRequired || context.HttpContext.User.IsAdmin()) return;

        var state = access.Status.At(DateTime.UtcNow);
        var method = context.HttpContext.Request.Method;
        var blocked = state == ServerState.Offline
                      || (state == ServerState.Maintenance && !HttpMethods.IsGet(method) && !HttpMethods.IsHead(method));
        if (blocked)
            context.Result = new ObjectResult(new { message = state == ServerState.Offline ? "The server is offline." : "Saving is paused during maintenance.", state })
                { StatusCode = StatusCodes.Status503ServiceUnavailable };
    }
}
