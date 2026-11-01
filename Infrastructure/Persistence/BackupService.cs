using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.Persistence;

/// <summary>
/// Backups are <c>wwwroot/backups/&lt;database&gt;-&lt;yyyyMMdd-HHmmss&gt;-&lt;kind&gt;.db3</c> (taiko-…), written with SQLite's
/// online backup API so the live database stays consistent while the server runs. The database's name keeps servers
/// sharing a folder (a test copy, say) out of each other's backups.
/// </summary>
public sealed partial class BackupService(IConfiguration configuration, ILogger<BackupService> logger) : IBackupService
{
    private static readonly string[] AutomaticKinds = ["scheduled", "update", "restore"];
    private static readonly string[] Kinds = ["manual", .. AutomaticKinds];
    private readonly SemaphoreSlim gate = new(1, 1);

    public string Folder => Path.Combine(PathHelper.GetRootPath(), "backups");

    private string DatabasePath => Path.Combine(PathHelper.GetRootPath(),
        configuration["DbFileName"] is { Length: > 0 } name ? name : PersistenceConstants.DefaultDbName);

    private string Stem => Path.GetFileNameWithoutExtension(DatabasePath);

    [GeneratedRegex(@"^(.+)-(\d{8}-\d{6})-([a-z]+)\.db3$")]
    private static partial Regex FileName();

    public IReadOnlyList<BackupItem> List()
    {
        if (!Directory.Exists(Folder)) return [];
        return Directory.EnumerateFiles(Folder, "*.db3")
            .Select(path => (Info: new FileInfo(path), Match: FileName().Match(Path.GetFileName(path))))
            .Where(x => x.Match.Success && x.Match.Groups[1].Value == Stem && Kinds.Contains(x.Match.Groups[3].Value))
            .Select(x => new BackupItem(x.Info.Name, x.Match.Groups[3].Value,
                DateTime.SpecifyKind(DateTime.ParseExact(x.Match.Groups[2].Value, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture), DateTimeKind.Utc),
                x.Info.Length))
            .OrderByDescending(b => b.CreatedAt).ToList();
    }

    public async Task<BackupItem> CreateAsync(string kind, CancellationToken cancellationToken = default)
    {
        if (!Kinds.Contains(kind)) throw new ArgumentException($"Unknown backup kind {kind}.", nameof(kind));
        await gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(Folder);
            var name = $"{Stem}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{kind}.db3";
            var path = Path.Combine(Folder, name);
            Copy(DatabasePath, path);
            logger.LogInformation("Backed up the database to {Backup}", name);
            return List().First(b => b.Name == name);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<bool> RestoreAsync(string name, CancellationToken cancellationToken = default)
    {
        if (PathOf(name) is not { } source) return false;
        await CreateAsync("restore", cancellationToken);
        await gate.WaitAsync(cancellationToken);
        try
        {
            // The backup API copies page by page under SQLite's own locks, so it's safe with the server's connections open.
            Copy(source, DatabasePath);
            logger.LogWarning("Restored the database from {Backup}", name);
            return true;
        }
        finally
        {
            gate.Release();
        }
    }

    public bool Delete(string name)
    {
        if (PathOf(name) is not { } path) return false;
        File.Delete(path);
        return true;
    }

    // Only names List() reports: nothing outside the backups folder.
    public string? PathOf(string name) => List().Any(b => b.Name == name) ? Path.Combine(Folder, name) : null;

    public void Prune(int keep)
    {
        foreach (var old in List().Where(b => AutomaticKinds.Contains(b.Kind)).GroupBy(b => b.Kind).SelectMany(g => g.Skip(Math.Max(1, keep))))
        {
            File.Delete(Path.Combine(Folder, old.Name));
            logger.LogInformation("Removed old backup {Backup}", old.Name);
        }
    }

    private static void Copy(string from, string to)
    {
        // No pooling: a pooled connection would keep the file open (and a backup undeletable).
        using var source = new SqliteConnection($"Data Source={from};Pooling=False");
        using var destination = new SqliteConnection($"Data Source={to};Pooling=False");
        source.Open();
        destination.Open();
        source.BackupDatabase(destination);
    }
}
