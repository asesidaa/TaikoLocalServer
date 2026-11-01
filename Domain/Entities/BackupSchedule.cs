namespace TaikoLocalServer.Domain.Entities;

/// <summary>Server Management › Backups' schedule. One row (Id 1), created with these defaults.</summary>
public class BackupSchedule
{
    public int Id { get; set; } = 1;
    public bool Daily { get; set; } = true;
    /// <summary>Local time of day, "HH:mm".</summary>
    public string Time { get; set; } = "04:00";
    /// <summary>How many of each automatic kind (scheduled, before update, before restore) to keep; manual ones stay.</summary>
    public int Keep { get; set; } = 7;
    /// <summary>Back up before database updates (migrations) at startup.</summary>
    public bool BeforeUpdates { get; set; } = true;
}
