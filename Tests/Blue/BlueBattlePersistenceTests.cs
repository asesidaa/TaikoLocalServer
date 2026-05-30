namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattlePersistenceTests
{
    private static readonly string[] ExpectedBattleTables =
    [
        "BlueBattleNpcStates",
        "BlueBattleReleaseStates",
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
            MaxDaniPower = 400,
            NpcCostumeFlg = [6, 7],
            SelectedSpecialId = 12,
            ReleaseSpecialFlg = [8, 9, 10],
            BondsLevel = 13,
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
            DaniPower = 600,
            TokenId = 301,
            TokenValue = 302
        });
        context.BlueBattleReleaseStates.Add(new BlueBattleReleaseState
        {
            Baid = 101,
            ReleaseInfoId = 12,
            ReleaseBattleStageId = 13,
            ReleaseNpcId = 14,
            ReleaseNpcCostumeId = 15,
            ReleaseNpcSpecialId = 16,
            AssignNextStageId = 17,
            TokenId = 301,
            TokenValue = 303,
            CreatedAt = now
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
        Assert.Equal(400u, npc.MaxDaniPower);
        Assert.Equal([6, 7], npc.NpcCostumeFlg);
        Assert.Equal(12u, npc.SelectedSpecialId);
        Assert.Equal([8, 9, 10], npc.ReleaseSpecialFlg);
        Assert.Equal(13u, npc.BondsLevel);

        var token = await reloaded.BlueBattleTokenStates.AsNoTracking().SingleAsync(row => row.Baid == 101 && row.TokenId == 301);
        Assert.Equal(302u, token.TokenValue);

        var stage = await reloaded.BlueBattleStageResults.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.True(stage.Id > 0);
        Assert.Equal(102u, stage.SongNo);
        Assert.Equal(201u, stage.NpcId);
        Assert.Equal(77u, stage.BossLife);

        var release = await reloaded.BlueBattleReleaseStates.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.True(release.Id > 0);
        Assert.Equal(12u, release.ReleaseInfoId);
        Assert.Equal(17u, release.AssignNextStageId);
        Assert.Equal(303u, release.TokenValue);
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
        context.BlueBattleReleaseStates.Add(new BlueBattleReleaseState
        {
            Baid = 102,
            ReleaseInfoId = 4,
            TokenId = 302,
            TokenValue = 9,
            CreatedAt = now
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
        Assert.Null(npc.MaxDaniPower);
        Assert.Null(npc.NpcCostumeFlg);
        Assert.Null(npc.SelectedSpecialId);
        Assert.Null(npc.ReleaseSpecialFlg);
        Assert.Null(npc.BondsLevel);

        var token = await context.BlueBattleTokenStates.AsNoTracking().SingleAsync(row => row.Baid == 103 && row.TokenId == 303);
        Assert.Null(token.TokenValue);
        Assert.Empty(await context.BlueBattleStageResults.Where(row => row.Baid == 103).ToListAsync());
        Assert.Empty(await context.BlueBattleReleaseStates.Where(row => row.Baid == 103).ToListAsync());
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

    private static string FindAddBlueBattleStateMigration()
    {
        var root = FindRepoRoot();
        var migrationFiles = Directory.GetFiles(
            Path.Combine(root, "Infrastructure", "Persistence", "Migrations"),
            "*_AddBlueBattleState.cs");

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
