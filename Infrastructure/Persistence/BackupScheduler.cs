using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Infrastructure.Persistence;

/// <summary>The daily backup: once a day after the schedule's time (local), then prunes to the newest Keep.</summary>
public sealed class BackupScheduler(IServiceScopeFactory scopes, IBackupService backups, ILogger<BackupScheduler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                await RunIfDue(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "Scheduled backup failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunIfDue(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        var schedule = await db.BackupSchedules.AsNoTracking().FirstOrDefaultAsync(cancellationToken) ?? new();
        if (!schedule.Daily || !TimeOnly.TryParse(schedule.Time, out var at)) return;

        // Today's slot (local time); due once it has passed and no scheduled backup is newer than it.
        var slot = DateTime.Today.Add(at.ToTimeSpan()).ToUniversalTime();
        if (DateTime.UtcNow < slot) slot = slot.AddDays(-1);
        if (backups.List().Any(b => b.Kind == "scheduled" && b.CreatedAt >= slot)) return;

        await backups.CreateAsync("scheduled", cancellationToken);
        backups.Prune(schedule.Keep);
    }
}
