using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

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
                    Stage(101, ghostStageData: new Ac15GreenGhostStageData
                    {
                        ArySectionData =
                        [
                            new(IsWin: true, GoodCnt: 10, OkCnt: 2, NgCnt: 1, PoundCnt: 4)
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

    [Fact]
    public async Task SaveAsync_MomoiroFavoritesAppendAfterCurrentDisplayOrder()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        database.Context.MomoiroFavoriteSongs.AddRange(
            new MomoiroFavoriteSongs { Baid = 1, SongNo = 300, DisplayOrder = 20 },
            new MomoiroFavoriteSongs { Baid = 1, SongNo = 100, DisplayOrder = 10 });
        await database.Context.SaveChangesAsync();

        await Ac15NormalPlayWriter.SaveAsync(
            database.Context,
            MomoiroTables(database.Context),
            new Ac15NormalPlayWriteRequest(
                1,
                PlayMode: 0,
                Stages: [Stage(200, isFavorite: true)],
                Ac15EraProfiles.Momoiro.Limits,
                DateTime.UnixEpoch),
            Ac15NormalStagePolicies.Standard,
            CancellationToken.None);

        var favorites = await database.Context.MomoiroFavoriteSongs
            .Where(song => song.Baid == 1)
            .OrderBy(song => song.SongNo)
            .ToArrayAsync();

        Assert.Collection(
            favorites,
            song =>
            {
                Assert.Equal(100u, song.SongNo);
                Assert.Equal(10, song.DisplayOrder);
            },
            song =>
            {
                Assert.Equal(200u, song.SongNo);
                Assert.Equal(21, song.DisplayOrder);
            },
            song =>
            {
                Assert.Equal(300u, song.SongNo);
                Assert.Equal(20, song.DisplayOrder);
            });
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

    private static Ac15NormalPlayTables<SongPlayDatumMomoiro, SongBestDatumMomoiro, MomoiroFavoriteSongs, MomoiroRecentSongs> MomoiroTables(TaikoDbContext context)
        => new(
            context.SongPlayDataMomoiro,
            context.SongBestDataMomoiro,
            context.MomoiroFavoriteSongs,
            context.MomoiroRecentSongs,
            CreateMomoiroSongPlayDatum,
            CreateMomoiroSongBestDatum,
            CreateFavorite: async (baid, songNo, cancellationToken) =>
            {
                var persistedMax = await context.MomoiroFavoriteSongs
                    .Where(song => song.Baid == baid)
                    .Select(song => (int?)song.DisplayOrder)
                    .MaxAsync(cancellationToken);
                var trackedMax = context.MomoiroFavoriteSongs.Local
                    .Where(song => song.Baid == baid)
                    .Select(song => (int?)song.DisplayOrder)
                    .DefaultIfEmpty()
                    .Max();

                return new MomoiroFavoriteSongs
                {
                    Baid = baid,
                    SongNo = songNo,
                    DisplayOrder = Math.Max(persistedMax ?? 0, trackedMax ?? 0) + 1
                };
            });

    private static SongPlayDatumMomoiro CreateMomoiroSongPlayDatum(Ac15PlayRow row)
        => new()
        {
            Baid = row.Baid,
            SongId = row.SongId,
            Difficulty = row.Difficulty,
            Crown = row.Crown,
            Score = row.Score,
            ScoreRate = row.ScoreRate,
            GoodCount = row.GoodCount,
            OkCount = row.OkCount,
            MissCount = row.MissCount,
            ComboCount = row.ComboCount,
            HitCount = row.HitCount,
            PoundCount = row.PoundCount,
            StarLevel = row.StarLevel,
            OptionFlg = row.OptionFlg,
            ToneFlg = row.ToneFlg,
            PlayMode = row.PlayMode,
            StageMode = row.StageMode,
            IsShin = row.IsShin,
            MusicCategory = row.MusicCategory,
            SelectedFolderId = row.SelectedFolderId,
            IsFavorite = row.IsFavorite,
            IsRecent = row.IsRecent,
            IsPapamama = row.IsPapamama,
            IsPushed = row.IsPushed,
            SoulGauge = row.SoulGauge,
            PlayDan = row.PlayDan,
            WaiwaiResult = row.WaiwaiResult,
            WaiwaiGauge = row.WaiwaiGauge,
            PlayTime = row.PlayTime
        };

    private static SongBestDatumMomoiro CreateMomoiroSongBestDatum(uint baid, Ac15BestRow row, bool allowCrownUpdate)
        => new()
        {
            Baid = baid,
            SongId = row.SongId,
            Difficulty = row.Difficulty,
            IsShin = row.IsShin,
            BestScore = row.BestScore,
            BestRate = row.BestRate,
            BestCrown = allowCrownUpdate ? row.BestCrown : CrownType.None
        };

    private static Ac15StageResult Stage(
        uint songNo,
        bool isFavorite = false,
        bool isRecent = false,
        Ac15GreenGhostStageData? ghostStageData = null)
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
            GreenGhostStage = ghostStageData
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
