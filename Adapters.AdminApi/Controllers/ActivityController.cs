namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Access › Activity: who did what, newest first. Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class ActivityController(ITaikoDbContext context) : BaseAdminController<ActivityController>
{
    [HttpGet]
    public async Task<ActionResult<ActivityResponse>> Get(long? before = null, int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 200);
        var rows = await context.Activity
            .Where(e => before == null || e.Id < before)
            .OrderByDescending(e => e.Id)
            .Take(limit + 1)
            .ToListAsync(HttpContext.RequestAborted);

        var more = rows.Count > limit;
        if (more) rows.RemoveAt(limit);

        var baids = rows.SelectMany(e => new[] { e.ActorBaid, e.TargetBaid }).OfType<uint>().Distinct().ToList();
        var names = await context.UserData.Where(u => baids.Contains(u.Baid))
            .ToDictionaryAsync(u => u.Baid, u => u.MyDonName, HttpContext.RequestAborted);
        string? Name(uint? baid) => baid is { } b ? names.GetValueOrDefault(b) : null;

        return Ok(new ActivityResponse(
            rows.Select(e => new ActivityItem(e.Id, DateTime.SpecifyKind(e.Time, DateTimeKind.Utc), e.ActorBaid, Name(e.ActorBaid),
                e.Action, e.TargetBaid, Name(e.TargetBaid), e.Detail)).ToList(),
            more ? rows[^1].Id : null));
    }
}
