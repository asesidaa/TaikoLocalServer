namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattlePersistenceTests
{
    private static readonly string[] ExpectedBattleTables =
    [
        "BlueBattleNpcStates",
        "BlueBattleStageResults",
        "BlueBattleTokenStates",
        "BlueBattleUserStates"
    ];

    [Fact]
    public async Task AddBlueBattleStateMigration_CreatesOnlyBlueBattleTablesAndUserRelationships()
    {
        var migrationSource = File.ReadAllText(FindAddBlueBattleStateMigration());

        foreach (var table in ExpectedBattleTables)
        {
            Assert.Contains($"name: \"{table}\"", migrationSource, StringComparison.Ordinal);
            Assert.Contains($"FK_{table}_UserData_Baid", migrationSource, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("migrationBuilder.Alter", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("migrationBuilder.AddColumn", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("migrationBuilder.DropColumn", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SongPlayDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SongBestDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("DanScoreDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("DanStageScoreDatum_Blue", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueFavoriteSongs", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueRecentSongs", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueShopSeasonStates", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueShopItemStates", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GreenGhostTokens", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GreenGhostWinnings", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GhostStageSectionDatum_Green", migrationSource, StringComparison.Ordinal);
    }

    [Fact]
    public void AddBlueBattleNpcSelectedSpecialsMigration_PreservesExistingSelectedSpecialAsSlot1()
    {
        var migrationSource = File.ReadAllText(FindMigration("AddBlueBattleNpcSelectedSpecials"));

        Assert.Contains("migrationBuilder.RenameColumn(", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"SelectedSpecialId\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("newName: \"SelectedSpecialId1\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"NpcCostumeId\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"SelectedSpecialId2\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"SelectedSpecialId3\"", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("newName: \"SelectedSpecialId3\"", migrationSource, StringComparison.Ordinal);
    }

    [Fact]
    public void RenameBlueBattleNpcMaxDpnMigration_RenamesDpnColumnsWithoutDroppingData()
    {
        var migrationSource = File.ReadAllText(FindMigration("RenameBlueBattleNpcMaxDpn"));

        Assert.Contains("migrationBuilder.RenameColumn(", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"MaxDaniPower\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("table: \"BlueBattleNpcStates\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("newName: \"MaxDpn\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"DaniPower\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("table: \"BlueBattleStageResults\"", migrationSource, StringComparison.Ordinal);
        Assert.Contains("newName: \"Dpn\"", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("migrationBuilder.DropColumn", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("migrationBuilder.AddColumn", migrationSource, StringComparison.Ordinal);
    }

    [Fact]
    public void RemoveBlueBattleReleaseStateMigration_DropsOnlyRedundantReleaseObservationTable()
    {
        var migrationSource = File.ReadAllText(FindMigration("RemoveBlueBattleReleaseState"));

        Assert.Contains("migrationBuilder.DropTable(", migrationSource, StringComparison.Ordinal);
        Assert.Contains("name: \"BlueBattleReleaseStates\"", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"BlueBattleUserStates\"", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"BlueBattleNpcStates\"", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"BlueBattleTokenStates\"", migrationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"BlueBattleStageResults\"", migrationSource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SqliteSchema_PersistsAndReloadsRepresentativeBlueBattleState()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        var now = new DateTime(2026, 5, 31, 10, 0, 0, DateTimeKind.Utc);
        await AddUserAsync(context, 101);

        context.BlueBattleUserStates.Add(new BlueBattleUserState
        {
            Baid = 101,
            ReleaseInfoFlg = [1, 2, 3],
            ReleaseBattleStageFlg = [4, 5],
            LastBattleStageId = 7,
            LastBossLife = 88,
            LastNpcId = 9,
            AssignStageId = 10,
            BattleBondsLvCap = 11,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleNpcStates.Add(new BlueBattleNpcState
        {
            Baid = 101,
            NpcId = 201,
            TotalExp = 300,
            MaxDpn = 400,
            NpcCostumeId = 401,
            NpcCostumeFlg = [6, 7],
            SelectedSpecialId1 = 12,
            SelectedSpecialId2 = 13,
            SelectedSpecialId3 = 14,
            ReleaseSpecialFlg = [8, 9, 10],
            BondsLevel = 15,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleTokenStates.Add(new BlueBattleTokenState
        {
            Baid = 101,
            TokenId = 301,
            TokenValue = 302,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleStageResults.Add(new BlueBattleStageResult
        {
            Baid = 101,
            CreatedAt = now,
            PlayDatetime = now,
            PlayMode = 2,
            StageMode = 3,
            StageIndex = 4,
            SongNo = 102,
            Level = 5,
            BattleStageId = 6,
            NpcId = 201,
            ResultType = 1,
            ClearFlag = 1,
            BossLife = 77,
            TotalExp = 500,
            AcquiredExp = 25,
            Dpn = 600,
            TokenId = 301,
            TokenValue = 302
        });
        await context.SaveChangesAsync();

        await using var reloaded = database.CreateContext();
        var user = await reloaded.BlueBattleUserStates.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.Equal([1, 2, 3], user.ReleaseInfoFlg);
        Assert.Equal([4, 5], user.ReleaseBattleStageFlg);
        Assert.Equal(7u, user.LastBattleStageId);
        Assert.Equal(88u, user.LastBossLife);
        Assert.Equal(9u, user.LastNpcId);
        Assert.Equal(10u, user.AssignStageId);
        Assert.Equal(11u, user.BattleBondsLvCap);

        var npc = await reloaded.BlueBattleNpcStates.AsNoTracking().SingleAsync(row => row.Baid == 101 && row.NpcId == 201);
        Assert.Equal(300u, npc.TotalExp);
        Assert.Equal(400u, npc.MaxDpn);
        Assert.Equal(401u, npc.NpcCostumeId);
        Assert.Equal([6, 7], npc.NpcCostumeFlg);
        Assert.Equal(12u, npc.SelectedSpecialId1);
        Assert.Equal(13u, npc.SelectedSpecialId2);
        Assert.Equal(14u, npc.SelectedSpecialId3);
        Assert.Equal([8, 9, 10], npc.ReleaseSpecialFlg);
        Assert.Equal(15u, npc.BondsLevel);

        var token = await reloaded.BlueBattleTokenStates.AsNoTracking().SingleAsync(row => row.Baid == 101 && row.TokenId == 301);
        Assert.Equal(302u, token.TokenValue);

        var stage = await reloaded.BlueBattleStageResults.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.True(stage.Id > 0);
        Assert.Equal(102u, stage.SongNo);
        Assert.Equal(201u, stage.NpcId);
        Assert.Equal(77u, stage.BossLife);
        Assert.Equal(600u, stage.Dpn);
    }

    [Fact]
    public async Task SqliteSchema_DoesNotCreateRedundantBlueBattleReleaseStatesTable()
    {
        await using var database = await CreateSchemaDatabaseAsync();

        await AssertTableAbsentAsync(database.Context, "BlueBattleReleaseStates");
    }

    [Fact]
    public async Task CreatingBlueBattleState_DoesNotCreateNormalBlueOrGreenAiBattleRows()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        var now = new DateTime(2026, 5, 31, 11, 0, 0, DateTimeKind.Utc);
        await AddUserAsync(context, 102);

        context.BlueBattleUserStates.Add(new BlueBattleUserState
        {
            Baid = 102,
            LastBattleStageId = 1,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleNpcStates.Add(new BlueBattleNpcState
        {
            Baid = 102,
            NpcId = 202,
            BondsLevel = 2,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleTokenStates.Add(new BlueBattleTokenState
        {
            Baid = 102,
            TokenId = 302,
            TokenValue = 9,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleStageResults.Add(new BlueBattleStageResult
        {
            Baid = 102,
            CreatedAt = now,
            BattleStageId = 3,
            NpcId = 202
        });
        await context.SaveChangesAsync();

        Assert.Equal(1, await context.BlueBattleUserStates.CountAsync(row => row.Baid == 102));
        await AssertNormalBlueAndGreenAiBattleStateEmptyAsync(context);
    }

    [Fact]
    public async Task UnresolvedBlueBattleFields_RemainNullOrAbsentUntilClientValuesAreStored()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        var now = new DateTime(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);
        await AddUserAsync(context, 103);

        context.BlueBattleUserStates.Add(new BlueBattleUserState
        {
            Baid = 103,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleNpcStates.Add(new BlueBattleNpcState
        {
            Baid = 103,
            NpcId = 203,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.BlueBattleTokenStates.Add(new BlueBattleTokenState
        {
            Baid = 103,
            TokenId = 303,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();

        var user = await context.BlueBattleUserStates.AsNoTracking().SingleAsync(row => row.Baid == 103);
        Assert.Null(user.ReleaseInfoFlg);
        Assert.Null(user.ReleaseBattleStageFlg);
        Assert.Null(user.LastBattleStageId);
        Assert.Null(user.LastBossLife);
        Assert.Null(user.LastNpcId);
        Assert.Null(user.AssignStageId);
        Assert.Null(user.BattleBondsLvCap);

        var npc = await context.BlueBattleNpcStates.AsNoTracking().SingleAsync(row => row.Baid == 103 && row.NpcId == 203);
        Assert.Null(npc.TotalExp);
        Assert.Null(npc.MaxDpn);
        Assert.Null(npc.NpcCostumeId);
        Assert.Null(npc.NpcCostumeFlg);
        Assert.Null(npc.SelectedSpecialId1);
        Assert.Null(npc.SelectedSpecialId2);
        Assert.Null(npc.SelectedSpecialId3);
        Assert.Null(npc.ReleaseSpecialFlg);
        Assert.Null(npc.BondsLevel);

        var token = await context.BlueBattleTokenStates.AsNoTracking().SingleAsync(row => row.Baid == 103 && row.TokenId == 303);
        Assert.Null(token.TokenValue);
        Assert.Empty(await context.BlueBattleStageResults.Where(row => row.Baid == 103).ToListAsync());
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

    private static async Task AssertNormalBlueAndGreenAiBattleStateEmptyAsync(TaikoDbContext context)
    {
        Assert.Empty(await context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await context.BlueRecentSongs.ToListAsync());
        Assert.Empty(await context.BlueFavoriteSongs.ToListAsync());
        Assert.Empty(await context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await context.DanStageScoreDataBlue.ToListAsync());
        Assert.Empty(await context.BlueShopSeasonStates.ToListAsync());
        Assert.Empty(await context.BlueShopItemStates.ToListAsync());
        Assert.Empty(await context.GhostStageSectionDataGreen.ToListAsync());
        Assert.Empty(await context.GreenGhostWinnings.ToListAsync());
        Assert.Empty(await context.GreenGhostTokens.ToListAsync());
    }

    private static async Task AssertTableAbsentAsync(TaikoDbContext context, string tableName)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);
        Assert.Equal(0L, count);
    }

    private static string FindAddBlueBattleStateMigration()
        => FindMigration("AddBlueBattleState");

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
