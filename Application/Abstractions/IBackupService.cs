using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>Database backups with SQLite's online backup API (never a raw copy of a file in use), in wwwroot/backups.</summary>
public interface IBackupService
{
    string Folder { get; }

    IReadOnlyList<BackupItem> List();

    /// <summary>Backs up the live database now; <paramref name="kind"/> is scheduled, manual, update or restore.</summary>
    Task<BackupItem> CreateAsync(string kind, CancellationToken cancellationToken = default);

    /// <summary>Copies a backup over the live database (after backing the current one up). False for an unknown name.</summary>
    Task<bool> RestoreAsync(string name, CancellationToken cancellationToken = default);

    bool Delete(string name);

    /// <summary>The file for a download, or null for an unknown name.</summary>
    string? PathOf(string name);

    /// <summary>Keeps the newest <paramref name="keep"/> of each automatic kind.</summary>
    void Prune(int keep);
}
