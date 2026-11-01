namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Access › Sign-ins: the Web UI sign-in log (90 days) and locked accounts. Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public class SignInsController(ITaikoDbContext context, IServerAccess access) : BaseAdminController<SignInsController>
{
    /// <param name="failed">true: only failures; false: only successful sign-ins; null: all.</param>
    [HttpGet]
    public async Task<SignInPage> Get(long? before = null, bool? failed = null, string? q = null, int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 200);
        var query = context.SignInAttempts.Where(a => a.Result != SignInOutcome.Unlocked && (before == null || a.Id < before));
        if (failed == true) query = query.Where(a => a.Result != SignInOutcome.SignedIn);
        if (failed == false) query = query.Where(a => a.Result == SignInOutcome.SignedIn);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            var baids = await context.UserData.Where(u => u.MyDonName.Contains(term)).Select(u => u.Baid).ToListAsync(HttpContext.RequestAborted);
            query = query.Where(a => a.Ip.Contains(term) || (a.Baid != null && baids.Contains(a.Baid.Value)));
        }
        var rows = await query.OrderByDescending(a => a.Id).Take(limit + 1).ToListAsync(HttpContext.RequestAborted);
        var more = rows.Count > limit;
        if (more) rows.RemoveAt(limit);
        var names = await Names(rows.Select(r => r.Baid));
        return new SignInPage(rows.Select(r => new SignInItem(r.Id, Utc(r.Time), r.Baid, r.Baid is { } b ? names.GetValueOrDefault(b) : null,
            r.AccessCode, r.Result, r.Ip, r.UserAgent)).ToList(), more ? rows[^1].Id : null);
    }

    [HttpGet("Summary")]
    public async Task<SignInSummary> Summary()
    {
        var now = DateTime.UtcNow;
        var rules = access.Rules;
        var failed = await context.SignInAttempts.CountAsync(a => a.Time > now.AddDays(-1) && a.Result != SignInOutcome.SignedIn && a.Result != SignInOutcome.Unlocked, HttpContext.RequestAborted);

        // Locked: enough wrong passwords in the lockout window since the last success or unlock (as AuthController checks).
        var since = now.AddMinutes(-rules.LockoutMinutes);
        var recent = await context.SignInAttempts.Where(a => a.Baid != null && a.Time > since).OrderBy(a => a.Time).ToListAsync(HttpContext.RequestAborted);
        var locked = recent.GroupBy(a => a.Baid!.Value).Select(g =>
        {
            var reset = g.LastOrDefault(a => a.Result is SignInOutcome.SignedIn or SignInOutcome.Unlocked)?.Time ?? DateTime.MinValue;
            var failures = g.Where(a => a.Result == SignInOutcome.WrongPassword && a.Time > reset).ToList();
            return failures.Count >= rules.LockoutAttempts ? (Baid: g.Key, Until: failures[rules.LockoutAttempts - 1].Time.AddMinutes(rules.LockoutMinutes)) : default;
        }).Where(x => x.Baid != 0).ToList();
        var names = await Names(locked.Select(l => (uint?)l.Baid));
        return new SignInSummary(failed, locked.Select(l => new LockedAccount(l.Baid, names.GetValueOrDefault(l.Baid) ?? "", Utc(l.Until))).ToList());
    }

    [HttpPost("Unlock/{baid}")]
    public async Task<IActionResult> Unlock(uint baid)
    {
        context.SignInAttempts.Add(new SignInAttempt { Time = DateTime.UtcNow, Baid = baid, Result = SignInOutcome.Unlocked });
        this.LogActivity(context, "user.unlock", baid);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<Dictionary<uint, string>> Names(IEnumerable<uint?> baids)
    {
        var ids = baids.OfType<uint>().Distinct().ToList();
        return await context.UserData.Where(u => ids.Contains(u.Baid)).ToDictionaryAsync(u => u.Baid, u => u.MyDonName, HttpContext.RequestAborted);
    }

    private static DateTime Utc(DateTime t) => DateTime.SpecifyKind(t, DateTimeKind.Utc);
}
