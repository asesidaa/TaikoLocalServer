namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>The Dashboard notice: everyone signed in reads it while it shows; admins edit it (Server Management › Announcements).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadProfiles, ApiScopes.AdminServer)]
public class NoticeController(ITaikoDbContext context) : BaseAdminController<NoticeController>
{
    /// <summary>The notice while it shows; 204 when there's none.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var n = await context.DashboardNotices.AsNoTracking().FirstOrDefaultAsync(HttpContext.RequestAborted);
        return n is { Show: true } && (n.Until is null || n.Until > DateTime.UtcNow) ? Ok(Settings(n)) : NoContent();
    }

    [HttpGet("Edit")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<DashboardNoticeSettings> GetForEdit() =>
        Settings(await context.DashboardNotices.AsNoTracking().FirstOrDefaultAsync(HttpContext.RequestAborted) ?? new DashboardNotice());

    [HttpPut]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> Put(DashboardNoticeSettings settings)
    {
        if (settings.Title.Length > 80 || settings.Message.Length > 1000)
            return BadRequest(new { message = "The title is at most 80 characters, the message 1,000." });
        if (settings.Show && settings.Title.Trim().Length == 0 && settings.Message.Trim().Length == 0)
            return BadRequest(new { message = "A notice that shows needs a title or a message." });

        var row = await context.DashboardNotices.FindAsync([1], HttpContext.RequestAborted);
        if (row is null) context.DashboardNotices.Add(row = new DashboardNotice());
        row.Show = settings.Show;
        row.Style = Enum.IsDefined(settings.Style) ? settings.Style : NoticeStyle.Info;
        row.Title = settings.Title.Trim();
        row.Message = settings.Message.Trim();
        row.Until = settings.Until?.ToUniversalTime();
        this.LogActivity(context, "notice.save", null, settings.Show ? row.Title : "hidden");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static DashboardNoticeSettings Settings(DashboardNotice n) => new()
    {
        Show = n.Show, Style = n.Style, Title = n.Title, Message = n.Message,
        Until = n.Until is { } u ? DateTime.SpecifyKind(u, DateTimeKind.Utc) : null,
    };
}
