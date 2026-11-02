using System.Text.Json;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowTokkunPersistenceTests
{
    [Fact]
    public async Task SqliteSchema_PersistsAndReloadsRepresentativeYellowTokkunState()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        await AddUserAsync(context, 101);

        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(101);
        save.TokkunTutorialFlg = 7;
        context.UserSaveDataYellow.Add(save);
        context.YellowTokkunStageResults.Add(new YellowTokkunStageResult
        {
            Baid = 101,
            PlayDatetime = "20260608120000",
            PlayMode = (uint)PlayMode.Tokkun,
            BanacoinDatetime = "20260608120102",
            TokkunSongCnt = 3,
            TookunSongnoesJson = JsonSerializer.Serialize(new uint[] { 101, 102, 101 }),
            TokkunSpeedchangeCnt = 4,
            TokkunAutoplayCnt = 5,
            TokkunJumpCnt = 6
        });
        await context.SaveChangesAsync();

        await using var reloaded = database.CreateContext();
        var reloadedSave = await reloaded.UserSaveDataYellow.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.Equal(7u, reloadedSave.TokkunTutorialFlg);

        var stage = await reloaded.YellowTokkunStageResults.AsNoTracking().SingleAsync(row => row.Baid == 101);
        Assert.True(stage.Id > 0);
        Assert.Equal("20260608120000", stage.PlayDatetime);
        Assert.Equal((uint)PlayMode.Tokkun, stage.PlayMode);
        Assert.Equal("20260608120102", stage.BanacoinDatetime);
        Assert.Equal(3u, stage.TokkunSongCnt);
        var tookunSongnoes = JsonSerializer.Deserialize<uint[]>(stage.TookunSongnoesJson);
        Assert.NotNull(tookunSongnoes);
        Assert.Equal([101u, 102u, 101u], tookunSongnoes);
        Assert.Equal(4u, stage.TokkunSpeedchangeCnt);
        Assert.Equal(5u, stage.TokkunAutoplayCnt);
        Assert.Equal(6u, stage.TokkunJumpCnt);
    }

    [Fact]
    public async Task UserSaveDataYellow_TokkunTutorialFlagPersistsNullableRawValues()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        await AddUserAsync(context, 102);
        await AddUserAsync(context, 103);

        var absent = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(102);
        var raw = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(103);
        raw.TokkunTutorialFlg = 7;
        context.UserSaveDataYellow.AddRange(absent, raw);
        await context.SaveChangesAsync();

        await using var reloaded = database.CreateContext();
        var reloadedAbsent = await reloaded.UserSaveDataYellow.AsNoTracking().SingleAsync(row => row.Baid == 102);
        var reloadedRaw = await reloaded.UserSaveDataYellow.AsNoTracking().SingleAsync(row => row.Baid == 103);

        Assert.Null(reloadedAbsent.TokkunTutorialFlg);
        Assert.Equal(7u, reloadedRaw.TokkunTutorialFlg);
    }

    [Fact]
    public async Task CreatingYellowTokkunHistory_DoesNotCreateNormalDaniShopOrCrossEraRows()
    {
        await using var database = await CreateSchemaDatabaseAsync();
        var context = database.Context;
        await AddUserAsync(context, 104);

        context.YellowTokkunStageResults.Add(new YellowTokkunStageResult
        {
            Baid = 104,
            PlayDatetime = "20260608120000",
            PlayMode = (uint)PlayMode.Tokkun,
            BanacoinDatetime = "20260608120102",
            TokkunSongCnt = 1,
            TookunSongnoesJson = JsonSerializer.Serialize(new uint[] { 201 }),
            TokkunSpeedchangeCnt = 0,
            TokkunAutoplayCnt = 0,
            TokkunJumpCnt = 0
        });
        await context.SaveChangesAsync();

        Assert.Equal(1, await context.YellowTokkunStageResults.CountAsync(row => row.Baid == 104));
        Assert.Empty(await context.SongPlayDataYellow.ToListAsync());
        Assert.Empty(await context.SongBestDataYellow.ToListAsync());
        Assert.Empty(await context.YellowRecentSongs.ToListAsync());
        Assert.Empty(await context.YellowFavoriteSongs.ToListAsync());
        Assert.Empty(await context.DanScoreDataYellow.ToListAsync());
        Assert.Empty(await context.DanStageScoreDataYellow.ToListAsync());
        Assert.Empty(await context.YellowShopSeasonStates.ToListAsync());
        Assert.Empty(await context.YellowShopItemStates.ToListAsync());
        Assert.Empty(await context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await context.BlueTokkunStageResults.ToListAsync());
        Assert.Empty(await context.BlueBattleStageResults.ToListAsync());
        Assert.Empty(await context.GreenShopSeasonStates.ToListAsync());
        Assert.Empty(await context.GreenShopItemStates.ToListAsync());
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
