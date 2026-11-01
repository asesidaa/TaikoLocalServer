using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Contracts.AdminApi.Authorization;

namespace TaikoLocalServer.Infrastructure.Identity;

/// <summary>
/// Every Web UI request: the token's session must still exist and not be revoked (Access › Sessions, Sign out
/// everywhere), and an Admin token must still belong to an admin. Either failing signs that device out.
/// </summary>
public static class SessionValidation
{
    // LastSeenAt is written at most this often, not on every request.
    private static readonly TimeSpan SeenGranularity = TimeSpan.FromMinutes(1);

    public static async Task OnTokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal!;
        if (!Guid.TryParse(principal.FindFirstValue(IJwtTokenService.SessionClaim), out var sid))
        {
            context.Fail("No session.");
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<ITaikoDbContext>();
        var session = await db.WebSessions.FindAsync([sid], context.HttpContext.RequestAborted);
        if (session is null || session.RevokedAt is not null || principal.GetBaid() != session.Baid)
        {
            context.Fail("Session ended.");
            return;
        }

        if (principal.IsAdmin() && !await db.UserData.AnyAsync(u => u.Baid == session.Baid && u.IsAdmin, context.HttpContext.RequestAborted))
        {
            context.Fail("No longer an admin.");
            return;
        }

        var now = DateTime.UtcNow;
        if (now - session.LastSeenAt > SeenGranularity)
        {
            session.LastSeenAt = now;
            session.Ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? session.Ip;
            await db.SaveChangesAsync(context.HttpContext.RequestAborted);
        }
    }
}
