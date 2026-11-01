using System.Security.Claims;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Signed-in Web UI devices (Access › Sessions; User Management's Sign out everywhere).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController(ITaikoDbContext context) : BaseAdminController<SessionsController>
{
    private Guid? Current => Guid.TryParse(User.FindFirstValue(IJwtTokenService.SessionClaim), out var sid) ? sid : null;

    /// <summary>Everyone's sessions (admins), or <paramref name="baid"/>'s (that user or an admin).</summary>
    [HttpGet]
    public async Task<ActionResult<List<SessionItem>>> Get(uint? baid = null)
    {
        if (Denied(baid) is { } denied) return denied;
        var since = DateTime.UtcNow.AddDays(-7);
        var rows = await context.WebSessions.Where(s => s.RevokedAt == null && s.LastSeenAt > since && (baid == null || s.Baid == baid))
            .OrderByDescending(s => s.LastSeenAt).ToListAsync(HttpContext.RequestAborted);
        var ids = rows.Select(r => r.Baid).Distinct().ToList();
        var names = await context.UserData.Where(u => ids.Contains(u.Baid)).ToDictionaryAsync(u => u.Baid, u => u.MyDonName, HttpContext.RequestAborted);
        var current = Current;
        return rows.Select(s => new SessionItem(s.Id, s.Baid, names.GetValueOrDefault(s.Baid) ?? "", Utc(s.CreatedAt), Utc(s.LastSeenAt),
            s.Ip, s.UserAgent, s.Id == current)).ToList();
    }

    /// <summary>Signs one device out (its owner or an admin).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var session = await context.WebSessions.FindAsync([id], HttpContext.RequestAborted);
        if (session is null) return NotFound();
        if (this.AuthorizeOwnerOrAdmin(session.Baid) is { } forbid) return forbid;
        session.RevokedAt = DateTime.UtcNow;
        if (session.Baid != User.GetBaid()) this.LogActivity(context, "session.revoke", session.Baid);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>Signs out every session except the caller's: all users' (admins), or one user's with <paramref name="baid"/>.</summary>
    [HttpPost("RevokeOthers")]
    public async Task<IActionResult> RevokeOthers(uint? baid = null)
    {
        if (Denied(baid) is { } denied) return denied;
        var current = Current;
        var now = DateTime.UtcNow;
        var sessions = await context.WebSessions.Where(s => s.RevokedAt == null && s.Id != current && (baid == null || s.Baid == baid))
            .ToListAsync(HttpContext.RequestAborted);
        sessions.ForEach(s => s.RevokedAt = now);
        this.LogActivity(context, "session.revokeAll", baid, $"{sessions.Count} sessions");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok(new { revoked = sessions.Count });
    }

    // One user's sessions: that user or an admin. Everyone's: admins only.
    private ActionResult? Denied(uint? baid) =>
        baid is { } b ? this.AuthorizeOwnerOrAdmin(b) : User.IsAdmin() ? null : Forbid();

    private static DateTime Utc(DateTime t) => DateTime.SpecifyKind(t, DateTimeKind.Utc);
}
