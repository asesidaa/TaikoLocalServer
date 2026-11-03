namespace TaikoLocalServer.Tests.Architecture;

// A fresh install runs every migration in id order. The merged branches' migrations were made against different models,
// and on SQLite a column change rebuilds the whole table from the model saved with that migration, so a later one can
// bring back columns an earlier one dropped. The migrated schema must be the one the model builds.
public sealed class MigrationsMatchModelTests
{
    [Fact]
    public async Task FreshMigratedDatabase_HasTheModelsTablesAndColumns()
    {
        // An old migration (SeparateTokens) reads through its own TaikoDbContext(), which opens wwwroot/taiko.db3 next to
        // the app, so the migrated side has to be that file (here, the test's own folder).
        var path = Path.Combine(PathHelper.GetRootPath(), "taiko.db3");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        SqliteConnection.ClearAllPools();
        File.Delete(path);
        Dictionary<string, HashSet<string>> migrated;
        try
        {
            await using (var db = new TaikoDbContext()) await db.Database.MigrateAsync();
            await using var file = new SqliteConnection($"Data Source={path}");
            await file.OpenAsync();
            migrated = await ReadAsync(file);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
        var model = await SchemaAsync(db => db.Database.EnsureCreatedAsync());

        var differences = model.Keys.Union(migrated.Keys).Order()
            .Where(table => !migrated.TryGetValue(table, out var m) || !model.TryGetValue(table, out var e) || !m.SetEquals(e))
            .Select(table => $"{table}: missing [{string.Join(", ", model.GetValueOrDefault(table, []).Except(migrated.GetValueOrDefault(table, [])))}]," +
                $" extra [{string.Join(", ", migrated.GetValueOrDefault(table, []).Except(model.GetValueOrDefault(table, [])))}]")
            .ToList();

        Assert.True(differences.Count == 0, string.Join("\n", differences));
    }

    // Each table's columns as "name type" plus "!" when NOT NULL.
    private static async Task<Dictionary<string, HashSet<string>>> SchemaAsync(Func<TaikoDbContext, Task> create)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using (var db = new TaikoDbContext(new DbContextOptionsBuilder<TaikoDbContext>().UseSqlite(connection).Options))
            await create(db);
        return await ReadAsync(connection);
    }

    private static async Task<Dictionary<string, HashSet<string>>> ReadAsync(SqliteConnection connection)
    {
        var tables = new List<string>();
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%'";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
        }

        var schema = new Dictionary<string, HashSet<string>>();
        foreach (var table in tables)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info(\"{table}\")";
            await using var reader = await cmd.ExecuteReaderAsync();
            var columns = new HashSet<string>();
            while (await reader.ReadAsync())
                columns.Add($"{reader.GetString(1)} {reader.GetString(2)}{(reader.GetInt64(3) == 1 ? "!" : "")}");
            schema[table] = columns;
        }
        return schema;
    }
}
