namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>What cabinets are told on each heartbeat: Operational, Pre-maintenance, Maintenance or Offline.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
[OpenInEveryServerState]
public class ServerStateController(IServerAccess access, ITaikoDbContext context) : BaseAdminController<ServerStateController>
{
    /// <summary>For everyone, signed in or not: the Web UI's banners and offline page (and its connection check).</summary>
    [HttpGet("Current")]
    [AllowAnonymous]
    public ServerStateSettings Current() => Get();

    [HttpGet]
    public ServerStateSettings Get()
    {
        var s = access.Status;
        return new ServerStateSettings
        {
            State = s.State, Start = Utc(s.Start), End = Utc(s.End), Current = s.At(DateTime.UtcNow),
        };
    }

    [HttpPut]
    public async Task<IActionResult> Put(ServerStateSettings settings)
    {
        if (!Enum.IsDefined(settings.State)) return BadRequest(new { message = "Unknown state." });
        var start = settings.Start?.ToUniversalTime();
        var end = settings.End?.ToUniversalTime();
        if (start is not null && end is not null && end <= start)
            return BadRequest(new { message = "The end must be after the start." });

        // Operational has no window.
        var operational = settings.State == ServerState.Operational;
        await access.UpdateStatusAsync(new ServerStatus
        {
            State = settings.State, Start = operational ? null : start, End = operational ? null : end,
        }, HttpContext.RequestAborted);
        this.LogActivity(context, "server.state", null, settings.State.ToString());
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static DateTime? Utc(DateTime? d) => d is { } v ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : null;
}
