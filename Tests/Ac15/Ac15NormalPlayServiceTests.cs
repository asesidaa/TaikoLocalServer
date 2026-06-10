using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15NormalPlayServiceTests
{
    [Fact]
    public async Task SaveAsync_ReturnsSuccessWhenBaidIsZero()
    {
        await using var database = await SchemaDatabase.CreateAsync();

        var result = await Ac15NormalPlayService.SaveAsync(
            database.Context,
            baid: 0,
            new CommonPlayResultData(),
            Ac15EraProfiles.Blue,
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
    }

    [Fact]
    public async Task SaveAsync_SkipsMissingUserWithSuccess()
    {
        await using var database = await SchemaDatabase.CreateAsync();

        var result = await Ac15NormalPlayService.SaveAsync(
            database.Context,
            baid: 1,
            new CommonPlayResultData(),
            Ac15EraProfiles.Blue,
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await database.Context.SongPlayDataBlue.ToListAsync());
    }

    [Fact]
    public async Task SaveAsync_AddsStageRowsAndTrimsRecentSongs()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        database.Context.BlueRecentSongs.AddRange(
            Enumerable.Range(0, Ac15EraProfiles.Blue.Limits.MaxRecentSongs - 1)
                .Select(index => new BlueRecentSongs
                {
                    Baid = 1,
                    SongNo = (uint)(900 + index),
                    LastPlayed = DateTime.UtcNow.AddMinutes(-index - 1)
                }));
        await database.Context.SaveChangesAsync();

        var playResult = new CommonPlayResultData
        {
            PlayDatetime = "20260607010101",
            PlayMode = 0,
            AryStageInfoes =
            [
                new()
                {
                    SongNo = 101,
                    Level = 2,
                    StageMode = 0,
                    PlayResult = 2,
                    PlayScore = 123456,
                    ScoreRate = 87,
                    IsFavorite = true,
                    IsRecent = true
                }
            ]
        };

        var result = await Ac15NormalPlayService.SaveAsync(
            database.Context,
            baid: 1,
            playResult,
            Ac15EraProfiles.Blue,
            DefaultAc15EraHooks.Instance,
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await database.Context.SongPlayDataBlue.Where(row => row.Baid == 1 && row.SongId == 101).ToListAsync());
        Assert.Single(await database.Context.SongBestDataBlue.Where(row => row.Baid == 1 && row.SongId == 101).ToListAsync());
        Assert.Single(await database.Context.BlueFavoriteSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());
        Assert.Single(await database.Context.BlueRecentSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());
        Assert.Equal(
            Ac15EraProfiles.Blue.Limits.MaxRecentSongs,
            await database.Context.BlueRecentSongs.CountAsync(row => row.Baid == 1));
    }

    private sealed class SchemaDatabase(SqliteConnection connection) : IAsyncDisposable
    {
        public TaikoDbContext Context { get; } = CreateContext(connection);

        public static async Task<SchemaDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SchemaDatabase(connection);
            await database.Context.Database.EnsureCreatedAsync();
            return database;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static TaikoDbContext CreateContext(SqliteConnection connection)
            => new(new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options);
    }
}
