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

    private static UpdatePlayResultCommandHandler CreateHandler(BlueHandlerFixture fixture)
    {
        return new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);
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

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
