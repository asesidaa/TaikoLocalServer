namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Server Management › Cabinets: every cabinet seen, who may connect, and per-cabinet settings. Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class CabinetsController(ICabinetRegistry cabinets, IGameDataCatalog catalog, ITaikoDbContext context) : BaseAdminController<CabinetsController>
{
    public record AccessRequest(CabinetAccess Access);
    public record PreApproveRequest(string ChassisId);

    // A cabinet sends a heartbeat every few minutes while it's on.
    private static readonly TimeSpan OnlineWindow = TimeSpan.FromMinutes(5);

    [HttpGet]
    public CabinetList Get() => new(cabinets.Restricted, cabinets.List().Select(Item).ToList());

    [HttpPut("{chassisId}/Access")]
    public async Task<IActionResult> SetAccess(string chassisId, AccessRequest request)
    {
        if (cabinets.Find(chassisId) is null) return NotFound();
        if (!Enum.IsDefined(request.Access)) return BadRequest();
        await cabinets.UpdateAsync(chassisId, c => c.Access = request.Access, HttpContext.RequestAborted);
        await Log($"cabinet.{request.Access.ToString().ToLowerInvariant()}", chassisId);
        return NoContent();
    }

    /// <summary>Allows a chassis ID before the cabinet ever connects (when only approved cabinets may).</summary>
    [HttpPost]
    public async Task<IActionResult> PreApprove(PreApproveRequest request)
    {
        var id = request.ChassisId.Trim();
        if (id.Length is 0 or > 64) return BadRequest(new { message = "Enter the cabinet's chassis ID." });
        await cabinets.UpdateAsync(id, c => c.Access = CabinetAccess.Allowed, HttpContext.RequestAborted);
        await Log("cabinet.allow", id);
        return NoContent();
    }

    [HttpPut("{chassisId}/Settings")]
    public async Task<IActionResult> SetSettings(string chassisId, CabinetSettingsRequest request)
    {
        if (cabinets.Find(chassisId) is null) return NotFound();
        if (request.DanSet is { } set && catalog.Nijiiro().GetDanSets().All(s => s.Id != set))
            return BadRequest(new { message = "Unknown Dani Dojo course set." });
        await cabinets.UpdateAsync(chassisId, c =>
        {
            c.OwnSettings = request.OwnSettings;
            c.DanSet = request.DanSet;
            c.Movies = string.Join(',', request.Movies.Distinct());
        }, HttpContext.RequestAborted);
        await Log("cabinet.settings", chassisId);
        return NoContent();
    }

    [HttpDelete("{chassisId}")]
    public async Task<IActionResult> Forget(string chassisId)
    {
        if (!await cabinets.RemoveAsync(chassisId, HttpContext.RequestAborted)) return NotFound();
        await Log("cabinet.forget", chassisId);
        return NoContent();
    }

    private static CabinetItem Item(Cabinet c) => new(c.ChassisId, c.ShopId, c.Version, Utc(c.FirstSeen), Utc(c.LastSeen),
        c.LastSeen is { } seen && DateTime.UtcNow - seen < OnlineWindow, c.Access, c.RefusedCount, c.OwnSettings, c.DanSet,
        c.Movies.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(uint.Parse).ToList());

    private static DateTime? Utc(DateTime? t) => t is { } v ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : null;

    private async Task Log(string action, string chassisId)
    {
        this.LogActivity(context, action, null, chassisId);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
    }
}
