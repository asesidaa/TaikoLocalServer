namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>The Dashboard's server strip for admins: the server state, cabinets online now and plays today.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer)]
public class DashboardController(ITaikoDbContext context, ICabinetRegistry cabinets, IServerAccess access) : BaseAdminController<DashboardController>
{
    // As Server Management › Cabinets counts "online".
    private static readonly TimeSpan OnlineWindow = TimeSpan.FromMinutes(5);

    [HttpGet("Server")]
    public async Task<DashboardServer> Server()
    {
        var now = DateTime.UtcNow;
        var list = cabinets.List();
        var today = DateTime.Today;
        return new DashboardServer(
            access.Status.At(now),
            list.Count(c => c.LastSeen is { } seen && now - seen < OnlineWindow),
            list.Count,
            await context.SongPlayDataNijiiro.CountAsync(p => p.PlayTime >= today, HttpContext.RequestAborted));
    }
}
