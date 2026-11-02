using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Infrastructure.Persistence;

public sealed class DatabaseStartupMigrator
{
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string MigrationLockTableName = "__EFMigrationsLock";

    private readonly ILogger<DatabaseStartupMigrator> logger;
    private readonly IClock clock;
    private readonly DatabaseStartupMigrationOptions options;

    public DatabaseStartupMigrator(
        ILogger<DatabaseStartupMigrator> logger,
        IClock clock,
        IOptions<DatabaseStartupMigrationOptions> options)
    {
        this.logger = logger;
        this.clock = clock;
        this.options = options.Value;
    }

    public DatabaseStartupMigrator(
        ILogger<DatabaseStartupMigrator> logger,
        IClock clock,
        DatabaseStartupMigrationOptions options)
        : this(logger, clock, Options.Create(options))
    {
    }

    public async Task MigrateAsync(TaikoDbContext db, CancellationToken cancellationToken = default)
    {
        var appliedMigrations = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
        var pendingMigrations = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
        var lockRows = await ReadSqliteMigrationLockRowsAsync(db, cancellationToken);
        var dataSource = db.Database.GetDbConnection().DataSource;

        logger.LogInformation(
            "Database startup migration check: DataSource={DataSource}, AppliedMigrations={AppliedMigrationCount}, PendingMigrations={PendingMigrationCount}, MigrationLockRows={MigrationLockRows}",
            dataSource,
            appliedMigrations.Length,
            pendingMigrations.Length,
            lockRows.Count);

        if (lockRows.Count > 0)
        {
            await HandleExistingSqliteMigrationLockAsync(
                db,
                dataSource,
                lockRows,
                pendingMigrations,
                cancellationToken);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.MigrationTimeout);

        try
        {
            await db.Database.MigrateAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Database migration did not complete within {options.MigrationTimeout}. " +
                $"DataSource={dataSource}. If SQLite table {MigrationLockTableName} contains an abandoned row, stop all server instances and clear it after verifying the database is not mid-migration.");
        }
    }

    private async Task HandleExistingSqliteMigrationLockAsync(
        TaikoDbContext db,
        string dataSource,
        IReadOnlyList<MigrationLockRow> lockRows,
        IReadOnlyList<string> pendingMigrations,
        CancellationToken cancellationToken)
    {
        if (pendingMigrations.Count > 0)
        {
            throw new InvalidOperationException(
                $"SQLite migration lock table {MigrationLockTableName} contains {lockRows.Count} row(s), " +
                $"but {pendingMigrations.Count} pending migration(s) remain ({string.Join(", ", pendingMigrations.Take(3))}). " +
                $"Startup will not clear this lock automatically because the database may be partially migrated. " +
                $"DataSource={dataSource}.");
        }

        if (!IsAbandoned(lockRows))
        {
            logger.LogWarning(
                "SQLite migration lock table {MigrationLockTableName} contains {MigrationLockRows} row(s), but the newest lock has not exceeded the abandoned-lock threshold of {AbandonedLockAge}. DataSource={DataSource}",
                MigrationLockTableName,
                lockRows.Count,
                options.AbandonedLockAge,
                dataSource);
            return;
        }

        logger.LogWarning(
            "Clearing abandoned SQLite migration lock in {MigrationLockTableName}. DataSource={DataSource}, LockRows={MigrationLockRows}, NewestLockTimestamp={NewestLockTimestamp}",
            MigrationLockTableName,
            dataSource,
            lockRows.Count,
            lockRows.Max(row => row.Timestamp));

        await ExecuteSqliteCommandAsync(
            db,
            $"DELETE FROM \"{MigrationLockTableName}\";",
            cancellationToken);
    }

    private bool IsAbandoned(IReadOnlyList<MigrationLockRow> lockRows)
    {
        if (lockRows.Any(row => row.Timestamp is null))
        {
            return false;
        }

        var now = new DateTimeOffset(DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc));
        var newestLockTimestamp = lockRows.Max(row => row.Timestamp!.Value);
        return now - newestLockTimestamp >= options.AbandonedLockAge;
    }

    private static async Task<IReadOnlyList<MigrationLockRow>> ReadSqliteMigrationLockRowsAsync(
        TaikoDbContext db,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(db.Database.ProviderName, SqliteProviderName, StringComparison.Ordinal))
        {
            return [];
        }

        if (!await SqliteTableExistsAsync(db, MigrationLockTableName, cancellationToken))
        {
            return [];
        }

        return await WithOpenConnectionAsync(
            db,
            async connection =>
            {
                var rows = new List<MigrationLockRow>();
                await using var command = connection.CreateCommand();
                command.CommandText = $"SELECT \"Id\", \"Timestamp\" FROM \"{MigrationLockTableName}\";";
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    var rawTimestamp = reader.GetString(1);
                    rows.Add(new MigrationLockRow(
                        Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture),
                        TryParseTimestamp(rawTimestamp),
                        rawTimestamp));
                }

                return rows;
            },
            cancellationToken);
    }

    private static async Task<bool> SqliteTableExistsAsync(
        TaikoDbContext db,
        string tableName,
        CancellationToken cancellationToken)
    {
        return await WithOpenConnectionAsync(
            db,
            async connection =>
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName;";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "$tableName";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);
                var result = await command.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt32(result, CultureInfo.InvariantCulture) > 0;
            },
            cancellationToken);
    }

    private static async Task ExecuteSqliteCommandAsync(
        TaikoDbContext db,
        string sql,
        CancellationToken cancellationToken)
    {
        await WithOpenConnectionAsync(
            db,
            async connection =>
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync(cancellationToken);
                return true;
            },
            cancellationToken);
    }

    private static async Task<T> WithOpenConnectionAsync<T>(
        TaikoDbContext db,
        Func<DbConnection, Task<T>> action,
        CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            return await action(connection);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static DateTimeOffset? TryParseTimestamp(string rawTimestamp)
        => DateTimeOffset.TryParse(
            rawTimestamp,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var parsed)
            ? parsed
            : null;

    private sealed record MigrationLockRow(int Id, DateTimeOffset? Timestamp, string RawTimestamp);
}
