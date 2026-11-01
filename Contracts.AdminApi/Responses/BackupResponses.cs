namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>A database backup in wwwroot/backups. Kind: scheduled, manual, update (before a database update) or restore (before a restore).</summary>
public record BackupItem(string Name, string Kind, DateTime CreatedAt, long Size);

public record BackupScheduleSettings(bool Daily, string Time, int Keep, bool BeforeUpdates);

public record BackupOverview(IReadOnlyList<BackupItem> Backups, BackupScheduleSettings Schedule, string Folder);
