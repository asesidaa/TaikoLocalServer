using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15NormalPlayWriterTests
{
    [Fact]
    public async Task SaveAsync_WritesOnlyTheBoundEraTables()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        await database.Context.SaveChangesAsync();

        var playTime = new DateTime(2026, 5, 14, 3, 24, 42);

        await Ac15NormalPlayWriter.SaveAsync(
            database.Context,
            BlueTables(database.Context),
            new Ac15NormalPlayWriteRequest(
                1,
                PlayMode: 0,
                Stages: [Stage(101, isFavorite: true, isRecent: true)],
                Ac15EraProfiles.Blue.Limits,
                playTime),
            Ac15NormalStagePolicies.Standard,
            CancellationToken.None);

        Assert.Single(await database.Context.SongPlayDataBlue.ToListAsync());
        Assert.Single(await database.Context.SongBestDataBlue.ToListAsync());
        Assert.Single(await database.Context.BlueFavoriteSongs.ToListAsync());
        Assert.Single(await database.Context.BlueRecentSongs.ToListAsync());
        Assert.Empty(await database.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await database.Context.SongPlayDataYellow.ToListAsync());
    }

    [Fact]
    public async Task SaveAsync_AllowsGreenGhostSectionsThroughAfterAddPlayRow()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        await database.Context.SaveChangesAsync();

        await Ac15NormalPlayWriter.SaveAsync(
            database.Context,
            GreenTables(database.Context),
            new Ac15NormalPlayWriteRequest(
                1,
                PlayMode: 0,
                Stages:
                [
                    Stage(101, ghostStageData: new CommonPlayResultData.GhostStageData
                    {
                        ArySectionData =
                        [
                            new() { IsWin = true, GoodCnt = 10, OkCnt = 2, NgCnt = 1, PoundCnt = 4 }
                        ]
                    })
                ],
                Ac15EraProfiles.Green.Limits,
                DateTime.UnixEpoch),
            Ac15NormalStagePolicies.Green,
            CancellationToken.None);

        Assert.Single(await database.Context.SongPlayDataGreen.ToListAsync());
        var section = Assert.Single(await database.Context.GhostStageSectionDataGreen.ToListAsync());
        Assert.True(section.IsWin);
        Assert.Equal(10u, section.GoodCount);
    }

    private static Ac15NormalPlayTables<SongPlayDatumBlue, SongBestDatumBlue, BlueFavoriteSongs, BlueRecentSongs> BlueTables(TaikoDbContext context)
        => new(
            context.SongPlayDataBlue,
            context.SongBestDataBlue,
            context.BlueFavoriteSongs,
            context.BlueRecentSongs,
            Ac15NormalPlayMapper.ToBlueSongPlayDatum,
            Ac15NormalPlayMapper.ToBlueSongBestDatum);

    private static Ac15NormalPlayTables<SongPlayDatumGreen, SongBestDatumGreen, GreenFavoriteSongs, GreenRecentSongs> GreenTables(TaikoDbContext context)
        => new(
            context.SongPlayDataGreen,
            context.SongBestDataGreen,
            context.GreenFavoriteSongs,
            context.GreenRecentSongs,
            Ac15NormalPlayMapper.ToGreenSongPlayDatum,
            Ac15NormalPlayMapper.ToGreenSongBestDatum,
            AfterAddPlayRow: (play, row) =>
            {
                if (row.GhostStageData is null)
                {
                    return;
                }

                uint sectionNo = 0;
                foreach (var section in row.GhostStageData.ArySectionData)
                {
                    context.GhostStageSectionDataGreen.Add(new GhostStageSectionDatumGreen
                    {
                        Parent = play,
                        SectionNo = sectionNo++,
                        IsWin = section.IsWin,
                        GoodCount = section.GoodCnt,
                        OkCount = section.OkCnt,
                        NgCount = section.NgCnt,
                        PoundCount = section.PoundCnt
                    });
                }
            });

    private static CommonPlayResultData.StageData Stage(
        uint songNo,
        bool isFavorite = false,
        bool isRecent = false,
        CommonPlayResultData.GhostStageData? ghostStageData = null)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            StageMode = 0,
            PlayResult = 2,
            PlayScore = 123456,
            ScoreRate = 87,
            IsFavorite = isFavorite,
            IsRecent = isRecent,
            GhostStageData = ghostStageData
        };

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
