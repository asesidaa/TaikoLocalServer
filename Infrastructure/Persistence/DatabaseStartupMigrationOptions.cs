namespace TaikoLocalServer.Infrastructure.Persistence;

public sealed class DatabaseStartupMigrationOptions
{
    public TimeSpan MigrationTimeout { get; set; } = TimeSpan.FromSeconds(60);

    public TimeSpan AbandonedLockAge { get; set; } = TimeSpan.FromMinutes(5);
}
