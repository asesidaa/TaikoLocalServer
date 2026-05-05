using Microsoft.Extensions.Options;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

public static class ControllerAuthorizationExtensions
{
    /// <summary>
    /// Resource-level guard for "current user owns the baid OR is admin". Always
    /// passes when authentication is disabled (local mode) so callers can use this
    /// uniformly without re-checking the toggle.
    /// </summary>
    /// <returns><c>null</c> when access is allowed; otherwise an <see cref="ActionResult"/>
    /// the caller should return immediately. Returning <see cref="ActionResult"/> (rather
    /// than <see cref="IActionResult"/>) lets callers with an <c>ActionResult&lt;T&gt;</c>
    /// signature return the value via the implicit conversion.</returns>
    public static ActionResult? AuthorizeOwnerOrAdmin(this ControllerBase controller, uint baid)
    {
        var authSettings = controller.HttpContext.RequestServices
            .GetRequiredService<IOptions<AuthSettings>>().Value;
        if (!authSettings.AuthenticationRequired)
        {
            return null;
        }

        var user = controller.User;
        if (user.IsAdmin())
        {
            return null;
        }

        return user.GetBaid() == baid ? null : new ForbidResult();
    }
}
