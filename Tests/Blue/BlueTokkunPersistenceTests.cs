using System.Text.Json;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueTokkunPersistenceTests
{
    [Fact]
    public void AddBlueTokkunStateMigration_AddsOnlyBlueTokkunState()
    {
        var migrationSource = File.ReadAllText(FindMigration("AddBlueTokkunState"));

        Assert.Contains("migrationBuilder.AddColumn<uint>(", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"TokkunTutorialFlg\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("table: \"UserSaveData_Blue\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("migrationBuilder.CreateTable(", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"BlueTokkunStageResults\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("TookunSongnoesJson = table.Column<string>", migrationSource, StringComparison.Ordinal);
        Assert.Contains("FK_BlueTokkunStageResults_UserData_Baid", migrationSource, StringComparison.Ordinal);
        Assert.Contains("IX_BlueTokkunStageResults_Baid_PlayDatetime", migrationSource, StringComparison.Ordinal);

        Assert.DoesNotContain("migrationBuilder.Alter", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SongPlayDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SongBestDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("DanScoreDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("DanStageScoreDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueBattle", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueShop", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Green", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Nijiiro", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("UploadedAtUtc", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("CreatedAt", migrationSource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SqliteSchema_PersistsAndReloadsRepresentativeBlueTokkunState()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        await AddUserAsync(context, 101);

        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(101);
        save.TokkunTutorialFlg = 7;
        context.UserSaveDataBlue.Add(save);
        context.BlueTokkunStageResults.Add(new BlueTokkunStageResult
        {
            Baid = 101,
            PlayDatetime = "20260606120000",
            PlayMode = (uint)PlayMode.Tokkun,
            BanacoinDatetime = "20260606120102",
            TokkunSongCnt = 3,
            TookunSongnoesJson = JsonSerializer.Serialize(new uint[] { 101, 102, 101 }),
            TokkunSpeedchangeCnt = 4,
            TokkunAutoplayCnt = 5,
            TokkunJumpCnt = 6
        });
        await context.SaveChangesAsync();

        await using var reloaded = database.CreateContext();
        var reloadedSave = await reloaded.UserSaveDataBlue.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.Equal(7u, reloadedSave.TokkunTutorialFlg);

        var stage = await reloaded.BlueTokkunStageResults.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.True(stage.Id > 0);
        Assert.Equal("20260606120000", stage.PlayDatetime);
        Assert.Equal((uint)PlayMode.Tokkun, stage.PlayMode);
        Assert.Equal("20260606120102", stage.BanacoinDatetime);
        Assert.Equal(3u, stage.TokkunSongCnt);
        var tookunSongnoes = JsonSerializer.Deserialize<uint[]>(stage.TookunSongnoesJson);
        Assert.NotNull(tookunSongnoes);
        Assert.Equal([101u, 102u, 101u], tookunSongnoes);
        Assert.Equal(4u, stage.TokkunSpeedchangeCnt);
        Assert.Equal(5u, stage.TokkunAutoplayCnt);
        Assert.Equal(6u, stage.TokkunJumpCnt);
    }

    [Fact]
    public async Task UserSaveDataBlue_TokkunTutorialFlagPersistsNullableRawValues()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        await AddUserAsync(context, 102);
        await AddUserAsync(context, 103);

        var absent = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(102);
        var raw = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(103);
        raw.TokkunTutorialFlg = 7;
        context.UserSaveDataBlue.AddRange(absent, raw);
        await context.SaveChangesAsync();

        await using var reloaded = database.CreateContext();
        var reloadedAbsent = await reloaded.UserSaveDataBlue.AsNoTracking().SingleAsync(row => row.Baid == 102);
        var reloadedRaw = await reloaded.UserSaveDataBlue.AsNoTracking().SingleAsync(row => row.Baid == 103);

        Assert.Null(reloadedAbsent.TokkunTutorialFlg);
        Assert.Equal(7u, reloadedRaw.TokkunTutorialFlg);
    }

    [Fact]
    public async Task CreatingBlueTokkunState_DoesNotCreateNormalBattleShopOrGreenRows()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        await AddUserAsync(context, 104);

        context.BlueTokkunStageResults.Add(new BlueTokkunStageResult
        {
            Baid = 104,
            PlayDatetime = "20260606120000",
            PlayMode = (uint)PlayMode.Tokkun,
            BanacoinDatetime = "20260606120102",
            TokkunSongCnt = 1,
            TookunSongnoesJson = JsonSerializer.Serialize(new uint[] { 201 }),
            TokkunSpeedchangeCnt = 0,
            TokkunAutoplayCnt = 0,
            TokkunJumpCnt = 0
        });
        await context.SaveChangesAsync();

        Assert.Equal(1, await context.BlueTokkunStageResults.CountAsync(row => row.Baid == 104));
        Assert.Empty(await context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await context.BlueRecentSongs.ToListAsync());
        Assert.Empty(await context.BlueFavoriteSongs.ToListAsync());
        Assert.Empty(await context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await context.DanStageScoreDataBlue.ToListAsync());
        Assert.Empty(await context.BlueBattleStageResults.ToListAsync());
        Assert.Empty(await context.BlueBattleUserStates.ToListAsync());
        Assert.Empty(await context.BlueBattleNpcStates.ToListAsync());
        Assert.Empty(await context.BlueBattleTokenStates.ToListAsync());
        Assert.Empty(await context.BlueShopSeasonStates.ToListAsync());
        Assert.Empty(await context.BlueShopItemStates.ToListAsync());
        Assert.Empty(await context.GhostStageSectionDataGreen.ToListAsync());
        Assert.Empty(await context.GreenGhostWinnings.ToListAsync());
        Assert.Empty(await context.GreenGhostTokens.ToListAsync());
    }

    private static async Task AddUserAsync(TaikoDbContext context, uint baid)
    {
        context.UserData.Add(new UserDatum
        {
            Baid = baid,
            MyDonName = $"Baid {baid}"
        });
        await context.SaveChangesAsync();
    }

    private static string FindMigration(string migrationName)
    {
        var root = FindRepoRoot();
        var migrationFiles = Directory.GetFiles(
            Path.Combine(root, "Infrastructure", "Persistence", "Migrations"),
            $"*_{migrationName}.cs");

        return Assert.Single(migrationFiles, path => !path.EndsWith(".Designer.cs", StringComparison.Ordinal));
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }

    private static async Task<SchemaDatabase> CreateSchemaDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var database = new SchemaDatabase(connection);
        await database.Context.Database.EnsureCreatedAsync();
        return database;
    }

    private sealed class SchemaDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        public SchemaDatabase(SqliteConnection connection)
        {
            this.connection = connection;
            Context = CreateContext();
        }

        public TaikoDbContext Context { get; }

        public TaikoDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options;
            return new TaikoDbContext(options);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
