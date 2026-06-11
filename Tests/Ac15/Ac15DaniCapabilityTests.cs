using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15DaniCapabilityTests
{
    [Fact]
    public async Task GetScoresAsync_ReadsOnlyTheBoundTableAndTruncatesByArrivalSongCount()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        database.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            MedleyUniqueId = 20005,
            ClearGrade = Ac15DanClearGrade.NormalClear,
            ArrivalSongCount = 1,
            SoulGaugeTotal = 150,
            ComboCountTotal = 300,
            DanStageScoreData =
            [
                StageBlue(0, 101, 1000),
                StageBlue(1, 102, 2000)
            ]
        });
        database.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            MedleyUniqueId = 30005,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            ArrivalSongCount = 1,
            SoulGaugeTotal = 999,
            ComboCountTotal = 999,
            DanStageScoreData = [StageGreen(0, 201, 9000)]
        });
        await database.Context.SaveChangesAsync();

        var scores = await Ac15DaniReadback.GetScoresAsync(
            BlueTables(database.Context),
            baid: 1,
            requestedDanIds: new HashSet<uint> { 5, 6 },
            knownChallengeLevels: new HashSet<uint> { 5 },
            CancellationToken.None);
        var response = Ac15DaniReadback.BuildResponse(scores);

        var dan = Assert.Single(response.AryDanScoreDatas);
        Assert.Equal(5u, dan.DanId);
        Assert.Equal(1u, dan.ArrivalSongCnt);
        var stage = Assert.Single(dan.AryDanScoreDataStages);
        Assert.Equal(1000u, stage.HighScore);
    }

    [Fact]
    public async Task SaveAsync_WritesOnlyTheBoundDaniTables()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        await database.Context.SaveChangesAsync();

        var saveState = new Ac15DaniSaveState(1, DisplayDan: 1, IsAutoCostumeOn: false, DanCostumeId: 36);
        Ac15DaniSaveUpdate? saveUpdate = null;

        await Ac15DaniWriter.SaveAsync(
            BlueTables(database.Context),
            PlayResultDanClear(danId: 5),
            Ac15EraProfiles.Blue.Limits,
            challenges: [new Ac15DaniChallenge(5, 20005)],
            saveState,
            update => saveUpdate = update,
            NullLogger.Instance,
            CancellationToken.None);
        await database.Context.SaveChangesAsync();

        Assert.Single(await database.Context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await database.Context.DanScoreDataGreen.ToListAsync());
        Assert.NotNull(saveUpdate);
        Assert.Equal(5u, saveUpdate!.GotDanMax);
        Assert.Equal(6u, saveUpdate.DisplayDan);
    }

    private static Ac15DaniTables<DanScoreDatumBlue, DanStageScoreDatumBlue> BlueTables(TaikoDbContext context)
        => new(
            context.DanScoreDataBlue,
            context.DanScoreDataBlue.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToBlueDanScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanScoreDatum,
            Ac15DaniMapper.ToBlueDanStageScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanStageScoreDatum);

    private static DanStageScoreDatumBlue StageBlue(uint index, uint songNo, uint score)
        => new()
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            StageIndex = index,
            SongNumber = songNo,
            PlayScore = score,
            HighScore = score,
            GoodCount = 10,
            OkCount = 2,
            BadCount = 1,
            DrumrollCount = 4,
            TotalHitCount = 13,
            ComboCount = 12
        };

    private static DanStageScoreDatumGreen StageGreen(uint index, uint songNo, uint score)
        => new()
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            StageIndex = index,
            SongNumber = songNo,
            PlayScore = score,
            HighScore = score,
            GoodCount = 10,
            OkCount = 2,
            BadCount = 1,
            DrumrollCount = 4,
            TotalHitCount = 13,
            ComboCount = 12
        };

    private static CommonPlayResultData PlayResultDanClear(uint danId)
        => new()
        {
            PlayMode = (uint)PlayMode.DanMode,
            DanResult = (uint)Ac15DanClearGrade.NormalClear,
            ComboCntTotal = 300,
            AryStageInfoes =
            [
                new()
                {
                    SongNo = 101,
                    Level = 1,
                    PlayScore = 1000,
                    GoodCnt = 10,
                    OkCnt = 2,
                    NgCnt = 1,
                    PoundCnt = 4,
                    HitCnt = 13,
                    ComboCnt = 12,
                    SoulGauge = 150,
                    PlayDan = danId
                }
            ]
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
