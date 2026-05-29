using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BluePlayResultHandlerTests
{
    [Fact]
    public async Task UpdatePlayResult_Blue_GuestBaidDoesNotSave()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            0,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 0,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_UnknownUserDoesNotSave()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            99,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 99,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_SavesPlayBestCountersAndUnlocks()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "2026-05-28 12:00:00",
                GetDonmedal = 10,
                GetKatsumedal = 2,
                GetToneNoes = [4],
                GetCostumeNo1s = [1],
                GetCostumeNo2s = [2],
                GetCostumeNo3s = [3],
                GetCostumeNo4s = [4],
                GetCostumeNo5s = [5],
                GetTitleNoes = [10],
                ReleaseSongNoes = [104],
                HasAryCurrentCostume = true,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 1,
                    Costume2 = 2,
                    Costume3 = 3,
                    Costume4 = 4,
                    Costume5 = 5
                },
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataBlue.Where(row => row.Baid == 1).ToListAsync());
        var best = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(765432u, best!.BestScore);
        Assert.Equal(CrownType.Gold, best.BestCrown);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(10u, save!.TotalGetDonmedal);
        Assert.Equal(2u, save.TotalGetKatsumedal);
        Assert.True(BitIsSet(save.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(save.ToneFlg, 4));
        Assert.True(BitIsSet(save.CostumeFlg1, 1));
        Assert.True(BitIsSet(save.CostumeFlg2, 2));
        Assert.True(BitIsSet(save.CostumeFlg3, 3));
        Assert.True(BitIsSet(save.CostumeFlg4, 4));
        Assert.True(BitIsSet(save.CostumeFlg5, 5));
        Assert.True(BitIsSet(save.TitleFlg, 10));
        Assert.Equal(1u, save.Costume1);
        Assert.Equal(1u, save.CategJpopCnt);
        Assert.Equal(1u, save.SongPushedCnt);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_StoresCompactProtocolPlayDatetimeWithoutWarning()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var logger = new RecordingLogger<UpdatePlayResultCommandHandler>();
        var handler = CreateHandler(fixture, logger);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260528120000",
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(new DateTime(2026, 5, 28, 12, 0, 0), save!.LastPlayDatetime);
        Assert.DoesNotContain(logger.Events, log => log.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_SavesNormalAndShinBestSeparately()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    CreateStage(101, 1, 0, score: 100000),
                    CreateStage(101, 1, 1, score: 200000)
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var normal = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, false);
        var shin = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, true);
        Assert.NotNull(normal);
        Assert.NotNull(shin);
        Assert.Equal(100000u, normal!.BestScore);
        Assert.Equal(200000u, shin!.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_UnsupportedStageModeSkipsStageAndReturnsSuccess()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    CreateStage(101, 1, 3),
                    CreateStage(102, 1, 4),
                    CreateStage(103, 1, 99)
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_FavoriteAndRecentUseBlueTables()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var stages = Enumerable.Range(101, 12)
            .Select(song => CreateStage((uint)song, 1, 0))
            .ToList();

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "2026-05-28 12:00:00",
                AryStageInfoes = stages
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(5, await fixture.Context.BlueFavoriteSongs.CountAsync(row => row.Baid == 1));
        Assert.Equal(10, await fixture.Context.BlueRecentSongs.CountAsync(row => row.Baid == 1));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniSavesDanRowsAndKeepsNormalStageRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260528120000",
                PlayMode = (uint)PlayMode.DanMode,
                DanResult = 2,
                ComboCntTotal = 300,
                AryStageInfoes =
                [
                    CreateDanStage(101, 1, 326090, 124, 14, 0, 139, 138, 277, 51),
                    CreateDanStage(102, 1, 593280, 230, 33, 3, 287, 156, 550, 99),
                    CreateDanStage(103, 1, 818490, 314, 49, 6, 342, 156, 705, 100)
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(3, await fixture.Context.SongPlayDataBlue.CountAsync(row => row.Baid == 1));
        var dan = await fixture.Context.DanScoreDataBlue
            .Include(row => row.DanStageScoreData)
            .SingleAsync(row => row.Baid == 1 && row.DanId == 1 && !row.IsExtra);
        Assert.Equal(20001u, dan.MedleyUniqueId);
        Assert.Equal(BlueDanClearGrade.GoldClear, dan.ClearGrade);
        Assert.Equal(3u, dan.ArrivalSongCount);
        Assert.Equal(100u, dan.SoulGaugeTotal);
        Assert.Equal(300u, dan.ComboCountTotal);
        Assert.Equal(3, dan.DanStageScoreData.Count);
        Assert.Equal(326090u, dan.DanStageScoreData.Single(stage => stage.StageIndex == 0).HighScore);
        var normalBest = await fixture.Context.SongBestDataBlue.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.Null(normalBest);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(1u, save!.GotDanMax);
        Assert.Equal(2u, save.DispTaikojukuDan);
        Assert.Equal(BlueDanClearGrade.GoldClear, BlueDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniRejectsInvalidDanResultButKeepsNormalPlaySave()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = (uint)PlayMode.DanMode,
                DanResult = 3,
                AryStageInfoes = [CreateDanStage(101, 1, 123, 1, 2, 3, 4, 5, 6, 7)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniUnknownDanSkipsOnlyDanRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = (uint)PlayMode.DanMode,
                DanResult = 1,
                AryStageInfoes = [CreateDanStage(101, 999, 123, 1, 2, 3, 4, 5, 6, 7)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniExtraClearUpdatesExtraFlagsOnly()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(new BlueHandlerFixture.TestBlueCatalog(
            taikojukuFileOrder:
            [
                new BlueTaikojukuEntry
                {
                    UniqueId = 20101,
                    ChallengeLevel = 101,
                    VerupNo = 0,
                    Songs = [new BlueTaikojukuSong { SongNo = 101, Level = 1 }]
                }
            ]));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = (uint)PlayMode.DanMode,
                DanResult = 2,
                AryStageInfoes = [CreateDanStage(101, 101, 100, 10, 2, 1, 4, 12, 13, 100)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var save = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(0u, save!.GotDanMax);
        Assert.Equal(1u, save.DispTaikojukuDan);
        Assert.Equal(BlueDanClearGrade.GoldClear, BlueDanHelpers.GetPackedGrade(save.GotDanExtraFlg, 0));
        Assert.Equal(BlueDanClearGrade.NotClear, BlueDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
    }

    [Theory]
    [InlineData(true, 36u, true)]
    [InlineData(false, 7u, false)]
    public async Task UpdatePlayResult_Blue_DaniClearAppliesSpecialDanCostumeOnlyWhenAutoCostumeOn(
        bool isAutoCostumeOn,
        uint expectedCostume,
        bool shouldUnlockDanCostume)
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.IsAutoCostumeOn = isAutoCostumeOn;
        save.Costume1 = 7;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = (uint)PlayMode.DanMode,
                DanResult = 1,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 7
                },
                AryStageInfoes = [CreateDanStage(101, 1, 100, 1, 2, 3, 4, 5, 6, 100)]
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.NotNull(reloaded);
        Assert.Equal(expectedCostume, reloaded!.Costume1);
        Assert.Equal(shouldUnlockDanCostume, BitIsSet(reloaded.CostumeFlg1, 36));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_DaniDuplicateSongsKeepStageIndexRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = (uint)PlayMode.DanMode,
                DanResult = 1,
                AryStageInfoes =
                [
                    CreateDanStage(101, 1, 100, 1, 2, 3, 4, 5, 6, 7),
                    CreateDanStage(101, 1, 200, 2, 3, 4, 5, 6, 7, 8)
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var rows = await fixture.Context.DanStageScoreDataBlue
            .Where(row => row.Baid == 1 && row.DanId == 1)
            .OrderBy(row => row.StageIndex)
            .ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(0u, rows[0].StageIndex);
        Assert.Equal(1u, rows[1].StageIndex);
        Assert.Equal(100u, rows[0].HighScore);
        Assert.Equal(200u, rows[1].HighScore);
    }

    private static UpdatePlayResultCommandHandler CreateHandler(
        BlueHandlerFixture fixture,
        ILogger<UpdatePlayResultCommandHandler>? logger = null)
    {
        return new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            logger ?? NullLogger<UpdatePlayResultCommandHandler>.Instance);
    }

    private static CommonPlayResultData.StageData CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint score = 765432)
    {
        return new CommonPlayResultData.StageData
        {
            SongNo = songNo,
            Level = level,
            StageMode = stageMode,
            PlayResult = 2,
            PlayScore = score,
            ScoreRate = 95,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            HitCnt = 123,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            SelectedFolderId = 9,
            SoulGauge = 100
        };
    }

    private static CommonPlayResultData.StageData CreateDanStage(
        uint songNo,
        uint danId,
        uint score,
        uint good,
        uint ok,
        uint bad,
        uint drumroll,
        uint combo,
        uint hits,
        uint soulGauge)
    {
        var stage = CreateStage(songNo, 1, 0, score);
        stage.PlayDan = danId;
        stage.PlayResult = 0;
        stage.GoodCnt = good;
        stage.OkCnt = ok;
        stage.NgCnt = bad;
        stage.PoundCnt = drumroll;
        stage.ComboCnt = combo;
        stage.HitCnt = hits;
        stage.SoulGauge = soulGauge;
        return stage;
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEvent> Events { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Events.Add(new LogEvent(logLevel, formatter(state, exception)));
        }
    }

    private sealed record LogEvent(LogLevel Level, string Message);
}
