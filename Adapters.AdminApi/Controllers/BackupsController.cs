using TaikoLocalServer.Infrastructure.Identity;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Server Management › Backups: list, back up now, download, restore, delete, and the schedule. Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class BackupsController(IBackupService backups, ITaikoDbContext context) : BaseAdminController<BackupsController>
{
    public record RestoreRequest(string Confirm);

    [HttpGet]
    public async Task<BackupOverview> Get()
    {
        var s = await context.BackupSchedules.AsNoTracking().FirstOrDefaultAsync(HttpContext.RequestAborted) ?? new BackupSchedule();
        return new BackupOverview(backups.List(), new BackupScheduleSettings(s.Daily, s.Time, s.Keep, s.BeforeUpdates), backups.Folder);
    }

    [HttpPost]
    public async Task<BackupItem> BackUpNow()
    {
        var item = await backups.CreateAsync("manual", HttpContext.RequestAborted);
        this.LogActivity(context, "backup.create", null, item.Name);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return item;
    }

    [HttpGet("{name}/Download")]
    public IActionResult Download(string name) =>
        backups.PathOf(name) is { } path ? PhysicalFile(path, "application/vnd.sqlite3", name) : NotFound();

    /// <summary>Replaces the live database with a backup (the current one is backed up first). Needs RESTORE typed.</summary>
    [HttpPost("{name}/Restore")]
    public async Task<IActionResult> Restore(string name, RestoreRequest request)
    {
        if (request.Confirm != "RESTORE") return BadRequest(new { message = "Type RESTORE to confirm." });
        if (!await backups.RestoreAsync(name, HttpContext.RequestAborted)) return NotFound();

        // The restored database has its own rules and signing key: reload them (Web UI sessions may end).
        await HttpContext.RequestServices.GetRequiredService<ServerAccess>().InitializeAsync();
        this.LogActivity(context, "backup.restore", null, name);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
        if (!backups.Delete(name)) return NotFound();
        this.LogActivity(context, "backup.delete", null, name);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPut("Schedule")]
    public async Task<IActionResult> SetSchedule(BackupScheduleSettings settings)
    {
        if (!TimeOnly.TryParse(settings.Time, out var time)) return BadRequest(new { message = "Time is HH:mm." });
        if (settings.Keep is < 1 or > 365) return BadRequest(new { message = "Keep between 1 and 365." });
        var row = await context.BackupSchedules.FindAsync([1], HttpContext.RequestAborted);
        if (row is null) context.BackupSchedules.Add(row = new BackupSchedule());
        row.Daily = settings.Daily;
        row.Time = time.ToString("HH:mm");
        row.Keep = settings.Keep;
        row.BeforeUpdates = settings.BeforeUpdates;
        this.LogActivity(context, "backup.schedule", null, settings.Daily ? $"daily at {row.Time}, keep {row.Keep}" : "off");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        backups.Prune(row.Keep);
        return NoContent();
    }
}
