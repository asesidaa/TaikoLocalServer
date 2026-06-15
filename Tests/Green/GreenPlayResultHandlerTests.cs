using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenPlayResultHandlerTests
{
    [Theory]
    [InlineData(1, Difficulty.Easy)]
    [InlineData(2, Difficulty.Normal)]
    [InlineData(3, Difficulty.Hard)]
    [InlineData(4, Difficulty.Oni)]
    [InlineData(5, Difficulty.UraOni)]
    public void MapDifficulty_UsesGreenCourseOrder(uint level, Difficulty expected)
    {
        Assert.Equal(expected, GreenPlayResultMapping.MapDifficulty(level));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void MapDifficulty_RejectsGreenCourseOutsideOneThroughFive(uint level)
    {
        Assert.Equal(Difficulty.None, GreenPlayResultMapping.MapDifficulty(level));
    }

    [Theory]
    [InlineData(0, CrownType.None)]
    [InlineData(1, CrownType.Clear)]
    [InlineData(2, CrownType.Gold)]
    [InlineData(3, CrownType.Dondaful)]
    [InlineData(99, CrownType.None)]
    public void MapCrown_MapsKnownValues(uint playResult, CrownType expected)
    {
        Assert.Equal(expected, GreenPlayResultMapping.MapCrown(playResult));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_SavesPlayAndBest()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "2026-05-12 12:00:00",
                GetDonmedal = 10,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 2,
                        PlayScore = 765432,
                        GoodCnt = 100,
                        OkCnt = 20,
                        NgCnt = 3,
                        PoundCnt = 4,
                        ComboCnt = 120,
                        HitCnt = 123,
                        OptionFlg = [1, 2, 3],
                        ToneFlg = [4],
                        IsFavorite = true,
                        IsRecent = true
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal((uint)1, result);
        Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal((uint)765432, best!.BestScore);
        Assert.Equal(CrownType.Gold, best.BestCrown);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_CompactTimestampMatchesSavePlayAndRecentRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260514032442",
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000,
                        IsRecent = true
                    }
                ]
            }),
            CancellationToken.None);

        var expected = new DateTime(2026, 5, 14, 3, 24, 42);
        var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        var play = await fixture.Context.SongPlayDataGreen.SingleAsync(row => row.Baid == 1 && row.SongId == 101);
        var recent = await fixture.Context.GreenRecentSongs.SingleAsync(row => row.Baid == 1 && row.SongNo == 101);

        Assert.Equal(1u, result);
        Assert.Equal(expected, save.LastPlayDatetime);
        Assert.Equal(expected, play.PlayTime);
        Assert.Equal(expected, recent.LastPlayed);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_GuestBaidDoesNotSave()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            0,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 0,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.UserSaveDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_SavesNormalAndShinBestSeparately()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260514032442",
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 229170,
                        GoodCnt = 49,
                        OkCnt = 12,
                        NgCnt = 1,
                        PoundCnt = 66,
                        ComboCnt = 54,
                        HitCnt = 127,
                        OptionFlg = [0, 0],
                        ToneFlg = new byte[16],
                        StarLevel = 2,
                        SoulGauge = 100
                    },
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 1,
                        PlayResult = 1,
                        PlayScore = 897650,
                        GoodCnt = 71,
                        OkCnt = 16,
                        NgCnt = 1,
                        PoundCnt = 73,
                        ComboCnt = 78,
                        HitCnt = 160,
                        OptionFlg = [0, 0],
                        ToneFlg = new byte[16],
                        StarLevel = 3,
                        SoulGauge = 100,
                        IsPapamama = true
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var normalBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, false);
        var shinBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, true);

        Assert.NotNull(normalBest);
        Assert.NotNull(shinBest);
        Assert.Equal(229170u, normalBest!.BestScore);
        Assert.Equal(897650u, shinBest!.BestScore);

        var plays = await fixture.Context.SongPlayDataGreen
            .Where(row => row.Baid == 1 && row.SongId == 101)
            .OrderBy(row => row.Id)
            .ToListAsync();

        Assert.Equal(2, plays.Count);
        Assert.False(plays[0].IsShin);
        Assert.Equal(0u, plays[0].StageMode);
        Assert.True(plays[1].IsShin);
        Assert.Equal(1u, plays[1].StageMode);
        Assert.True(plays[1].IsPapamama);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_SkipsUnsupportedNormalStagesAndPersistsValidStages()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260514032442",
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000
                    },
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 102,
                        Level = 1,
                        StageMode = 2,
                        PlayResult = 1,
                        PlayScore = 9000
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Equal(101u, play.SongId);
        Assert.Empty(await fixture.Context.SongBestDataGreen.Where(row => row.SongId == 102).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AllUnsupportedNormalStagesReturnSuccessWithoutMutation()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 1024,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 9000
                    },
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 2,
                        PlayResult = 1,
                        PlayScore = 9000
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.GreenRecentSongs.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_SkipsUnknownStageModeWithoutMutation()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 2,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_SkipsOutOfPackedSongNoWithoutMutation()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 1024,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 123
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.GreenFavoriteSongs.ToListAsync());
        Assert.Empty(await fixture.Context.GreenRecentSongs.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsKnownRangeSongMissingFromLocalCatalog()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 122,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 123
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.SongId == 122).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsUnknownPlayResultAsNoCrown()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 99,
                        PlayScore = 123
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Equal(CrownType.None, play.Crown);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.None, best!.BestCrown);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_SkipsStageLevelOutsideOneThroughFiveWithoutMutation()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 0,
                        PlayResult = 1,
                        PlayScore = 123
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_MissingCurrentCostumeDoesNotClearSavedCostume()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.Costume1 = 7;
        save.Costume2 = 8;
        save.Costume3 = 9;
        save.Costume4 = 10;
        save.Costume5 = 11;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                HasAryCurrentCostume = false,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 123
                    }
                ]
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.Equal((uint)1, result);
        Assert.Equal((uint)7, reloaded!.Costume1);
        Assert.Equal((uint)8, reloaded.Costume2);
        Assert.Equal((uint)9, reloaded.Costume3);
        Assert.Equal((uint)10, reloaded.Costume4);
        Assert.Equal((uint)11, reloaded.Costume5);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AutoCostumeOffPreservesCurrentCostumeAndIgnoresCurrentCostumeUnlock()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.IsAutoCostumeOn = false;
        save.Costume1 = 7;
        save.Costume2 = 8;
        save.Costume3 = 9;
        save.Costume4 = 10;
        save.Costume5 = 11;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                HasAryCurrentCostume = true,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 108,
                    Costume2 = 109,
                    Costume3 = 110,
                    Costume4 = 111,
                    Costume5 = 112
                },
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, result);
        Assert.Equal(7u, reloaded.Costume1);
        Assert.Equal(8u, reloaded.Costume2);
        Assert.Equal(9u, reloaded.Costume3);
        Assert.Equal(10u, reloaded.Costume4);
        Assert.Equal(11u, reloaded.Costume5);
        Assert.False(BitIsSet(reloaded.CostumeFlg1, 108));
        Assert.False(BitIsSet(reloaded.CostumeFlg2, 109));
        Assert.False(BitIsSet(reloaded.CostumeFlg3, 110));
        Assert.False(BitIsSet(reloaded.CostumeFlg4, 111));
        Assert.False(BitIsSet(reloaded.CostumeFlg5, 112));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AutoCostumeOffStillAppliesExplicitCostumeRewards()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.IsAutoCostumeOn = false;
        save.Costume1 = 7;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                HasAryCurrentCostume = true,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 109
                },
                GetCostumeNo1s = [108],
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, result);
        Assert.Equal(7u, reloaded.Costume1);
        Assert.True(BitIsSet(reloaded.CostumeFlg1, 108));
        Assert.False(BitIsSet(reloaded.CostumeFlg1, 109));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_OmittedDifficultyPlayedFieldsPreserveExistingValues()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.DifficultyPlayedCourse = 3;
        save.DifficultyPlayedStar = 4;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                HasDifficultyPlayedCourse = false,
                HasDifficultyPlayedStar = false,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 123
                    }
                ]
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.Equal((uint)1, result);
        Assert.Equal((uint)3, reloaded!.DifficultyPlayedCourse);
        Assert.Equal((uint)4, reloaded.DifficultyPlayedStar);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DoesNotOverflowMedalTotals()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = uint.MaxValue;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                GetDonmedal = 1
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.Equal(1u, result);
        Assert.Equal(uint.MaxValue, reloaded!.TotalGetDonmedal);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsNewlyAwardedRewardIds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                GetToneNoes = [4],
                GetCostumeNo1s = [1, 43, 3, 44],
                GetTitleNoes = [106, 132, 144, 151, 158, 181],
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 229170,
                        GoodCnt = 49,
                        OkCnt = 12,
                        NgCnt = 1,
                        PoundCnt = 66,
                        ComboCnt = 54,
                        HitCnt = 127,
                        OptionFlg = [0, 0],
                        ToneFlg = new byte[16]
                    }
                ]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);

        Assert.Equal(1u, result);
        Assert.True(BitIsSet(save!.ToneFlg, 4));
        Assert.True(BitIsSet(save.CostumeFlg1, 1));
        Assert.True(BitIsSet(save.CostumeFlg1, 43));
        Assert.True(BitIsSet(save.CostumeFlg1, 44));
        Assert.True(BitIsSet(save.TitleFlg, 106));
        Assert.True(BitIsSet(save.TitleFlg, 181));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_IncrementsGenreAndSongCounters()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "2026-05-15 12:00:00",
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 100000,
                        GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                        OptionFlg = [0], ToneFlg = [0],
                        MusicCateg = 1,
                        IsPushed = true,
                        IsFavorite = true,
                        IsRecent = false
                    },
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 102,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 100000,
                        GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                        OptionFlg = [0], ToneFlg = [0],
                        MusicCateg = 2,
                        IsRecent = true
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, save.CategJpopCnt);
        Assert.Equal(1u, save.CategAnimeCnt);
        Assert.Equal(0u, save.CategGameCnt);
        Assert.Equal(1u, save.SongPushedCnt);
        Assert.Equal(1u, save.SongFavoriteCnt);
        Assert.Equal(1u, save.SongRecentCnt);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_MusicCategEightIncrementsNamcoCounter()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "2026-05-15 12:00:00",
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 100000,
                        GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                        OptionFlg = [0], ToneFlg = [0],
                        MusicCateg = 8
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Equal(8u, play.MusicCategory);
        var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.Equal(0u, save.CategJpopCnt);
        Assert.Equal(0u, save.CategAnimeCnt);
        Assert.Equal(0u, save.CategDoyoCnt);
        Assert.Equal(0u, save.CategVocaloidCnt);
        Assert.Equal(0u, save.CategGameCnt);
        Assert.Equal(1u, save.CategNamcoCnt);
        Assert.Equal(0u, save.CategVarietyCnt);
        Assert.Equal(0u, save.CategClassicCnt);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsMusicCategWithoutProfileCounter()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "2026-05-15 12:00:00",
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 100000,
                        GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                        OptionFlg = [0], ToneFlg = [0],
                        MusicCateg = 9
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Equal(9u, play.MusicCategory);
        var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.Equal(0u, save.CategJpopCnt);
        Assert.Equal(0u, save.CategAnimeCnt);
        Assert.Equal(0u, save.CategDoyoCnt);
        Assert.Equal(0u, save.CategVocaloidCnt);
        Assert.Equal(0u, save.CategGameCnt);
        Assert.Equal(0u, save.CategNamcoCnt);
        Assert.Equal(0u, save.CategVarietyCnt);
        Assert.Equal(0u, save.CategClassicCnt);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_PersistsIsPushedOnPlayLog()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "2026-05-15 12:00:00",
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        PlayResult = 1,
                        PlayScore = 100000,
                        GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
                        OptionFlg = [0], ToneFlg = [0],
                        MusicCateg = 0,
                        IsPushed = true
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = await fixture.Context.SongPlayDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.True(play.IsPushed);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_RecentSongsUpsertsForEveryStage_OrderedByPlayTime()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(1, GameEra.Green, new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = "2026-05-15 09:00:00",
            AryStageInfoes = [PlainStage(songNo: 101)]
        }), CancellationToken.None);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(1, GameEra.Green, new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = "2026-05-15 12:00:00",
            AryStageInfoes = [PlainStage(songNo: 102)]
        }), CancellationToken.None);

        var recents = await fixture.Context.GreenRecentSongs
            .Where(s => s.Baid == 1)
            .OrderByDescending(s => s.LastPlayed)
            .ToListAsync();
        Assert.Equal(2, recents.Count);
        Assert.Equal(102u, recents[0].SongNo);
        Assert.Equal(101u, recents[1].SongNo);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_RecentSongsTrimToTen()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        for (var i = 0; i < 11; i++)
        {
            await handler.Handle(Ac15PlayResultTestFactory.FromCommon(1, GameEra.Green, new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = new DateTime(2026, 5, 15, 9, 0, 0).AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss"),
                AryStageInfoes = [PlainStage(songNo: (uint)(101 + i))]
            }), CancellationToken.None);
        }

        var recents = await fixture.Context.GreenRecentSongs
            .Where(s => s.Baid == 1)
            .OrderByDescending(s => s.LastPlayed)
            .ToListAsync();
        Assert.Equal(10, recents.Count);
        Assert.Equal(111u, recents[0].SongNo);
        Assert.Equal(102u, recents[9].SongNo);
        Assert.DoesNotContain(recents, r => r.SongNo == 101u);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_FavoritesCapAtEraLimit()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        for (var i = 0; i < Ac15EraProfiles.Green.Limits.MaxFavoriteSongs + 1; i++)
        {
            await handler.Handle(Ac15PlayResultTestFactory.FromCommon(1, GameEra.Green, new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = new DateTime(2026, 5, 15, 9, 0, 0).AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss"),
                AryStageInfoes = [PlainStage(songNo: (uint)(101 + i), isFavorite: true)]
            }), CancellationToken.None);
        }

        var favorites = await fixture.Context.GreenFavoriteSongs
            .Where(s => s.Baid == 1)
            .ToListAsync();
        Assert.Equal(Ac15EraProfiles.Green.Limits.MaxFavoriteSongs, favorites.Count);
        Assert.DoesNotContain(favorites, f => f.SongNo == 111u);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_FavoritesRemovalWorksWhenAtCap()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        for (var i = 0; i < Ac15EraProfiles.Green.Limits.MaxFavoriteSongs; i++)
        {
            fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = (uint)(101 + i) });
        }
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(1, GameEra.Green, new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = "2026-05-15 12:00:00",
            AryStageInfoes = [PlainStage(songNo: 101, isFavorite: false)]
        }), CancellationToken.None);

        var favorites = await fixture.Context.GreenFavoriteSongs.Where(s => s.Baid == 1).ToListAsync();
        Assert.Equal(Ac15EraProfiles.Green.Limits.MaxFavoriteSongs - 1, favorites.Count);
        Assert.DoesNotContain(favorites, f => f.SongNo == 101u);
    }

    [Fact]
    public async Task GreenRecommend_ReturnsCatalogValues()
    {
        var greenCatalog = new GreenHandlerFixture.TestGreenCatalog
        {
            Recommend = new GreenRecommendEntry
            {
                RecommendSong = 101,
                RecommendBestSongs = [101, 102]
            }
        };
        await using var fixture = await GreenHandlerFixture.CreateAsync(greenCatalog);

        var handler = new GetRecommendQueryHandler(
            NullLogger<GetRecommendQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(
            new GetRecommendQuery(GameEra.Green, GenderType: 0, PlayerAge: 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(101u, response.RecommendSong);
        Assert.Equal(new List<uint> { 101, 102 }, response.RecommendBestSong);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_IgnoresOutOfRangeRewardIds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                GetTitleNoes = [(uint)GreenProtocolBytes.TitleFlagBytes * 8],
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Single(await fixture.Context.SongPlayDataGreen.ToListAsync());
        var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.All(save.TitleFlg, value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsCurrentCostumeOutsideSavedUnlockFlags()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                HasAryCurrentCostume = true,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 108,
                    Costume2 = 0,
                    Costume3 = 0,
                    Costume4 = 0,
                    Costume5 = 0
                },
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, result);
        Assert.Equal(108u, save.Costume1);
        Assert.True(BitIsSet(save.CostumeFlg1, 108));
    }

    [Fact]
    public async Task GreenDanSchema_CanInsertParentAndStageRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();

        fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 1,
            IsExtra = false,
            MedleyUniqueId = 20001,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            ArrivalSongCount = 1,
            SoulGaugeTotal = 100,
            ComboCountTotal = 138,
            DanStageScoreData =
            [
                new DanStageScoreDatumGreen
                {
                    Baid = 1,
                    DanId = 1,
                    IsExtra = false,
                    StageIndex = 0,
                    SongNumber = 790,
                    PlayScore = 326090,
                    HighScore = 326090,
                    GoodCount = 124,
                    OkCount = 14,
                    BadCount = 0,
                    DrumrollCount = 139,
                    TotalHitCount = 277,
                    ComboCount = 138
                }
            ]
        });

        await fixture.Context.SaveChangesAsync();

        var saved = await fixture.Context.DanScoreDataGreen
            .Include(row => row.DanStageScoreData)
            .SingleAsync(row => row.Baid == 1 && row.DanId == 1 && !row.IsExtra);

        Assert.Equal(Ac15DanClearGrade.GoldClear, saved.ClearGrade);
        var stage = Assert.Single(saved.DanStageScoreData);
        Assert.Equal(0u, stage.StageIndex);
        Assert.Equal(790u, stage.SongNumber);
    }

    [Fact]
    public async Task GreenPlayLog_StoresIsPushed()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();

        fixture.Context.SongPlayDataGreen.Add(new SongPlayDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            Crown = CrownType.Clear,
            Score = 100000,
            ScoreRate = 80,
            GoodCount = 10,
            OkCount = 2,
            MissCount = 1,
            ComboCount = 12,
            HitCount = 13,
            PoundCount = 0,
            StarLevel = 3,
            SupportLevel = 0,
            OptionFlg = [0],
            ToneFlg = [0],
            PlayMode = 0,
            StageMode = 0,
            IsShin = false,
            MusicCategory = 0,
            SelectedFolderId = 0,
            IsFavorite = false,
            IsRecent = false,
            IsPapamama = false,
            IsPushed = true,
            SoulGauge = 100,
            PlayDan = 0,
            WaiwaiResult = 0,
            WaiwaiGauge = 0,
            PlayTime = new DateTime(2026, 5, 15, 12, 0, 0)
        });
        await fixture.Context.SaveChangesAsync();

        var saved = await fixture.Context.SongPlayDataGreen.SingleAsync(row => row.Baid == 1);
        Assert.True(saved.IsPushed);
    }

    [Fact]
    public async Task GreenRecentSongs_StoresLastPlayed()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();

        var when = new DateTime(2026, 5, 15, 12, 0, 0);
        fixture.Context.GreenRecentSongs.Add(new GreenRecentSongs
        {
            Baid = 1,
            SongNo = 101,
            LastPlayed = when
        });
        await fixture.Context.SaveChangesAsync();

        var saved = await fixture.Context.GreenRecentSongs.SingleAsync(row => row.Baid == 1);
        Assert.Equal(when, saved.LastPlayed);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniPlaySavesDanDataWithoutNormalBest()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260515060858",
                PlayMode = 1,
                DanResult = 2,
                AryStageInfoes =
                [
                    new() { SongNo = 101, Level = 1, PlayResult = 0, PlayScore = 326090, GoodCnt = 124, OkCnt = 14, NgCnt = 0, PoundCnt = 139, ComboCnt = 138, HitCnt = 277, PlayDan = 1, SoulGauge = 51 },
                    new() { SongNo = 102, Level = 1, PlayResult = 0, PlayScore = 593280, GoodCnt = 230, OkCnt = 33, NgCnt = 3, PoundCnt = 287, ComboCnt = 156, HitCnt = 550, PlayDan = 1, SoulGauge = 99 },
                    new() { SongNo = 103, Level = 1, PlayResult = 0, PlayScore = 818490, GoodCnt = 314, OkCnt = 49, NgCnt = 6, PoundCnt = 342, ComboCnt = 156, HitCnt = 705, PlayDan = 1, SoulGauge = 100 }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var dan = await fixture.Context.DanScoreDataGreen
            .Include(row => row.DanStageScoreData)
            .SingleAsync(row => row.Baid == 1 && row.DanId == 1 && !row.IsExtra);

        Assert.Equal(20001u, dan.MedleyUniqueId);
        Assert.Equal(Ac15DanClearGrade.GoldClear, dan.ClearGrade);
        Assert.Equal(3u, dan.ArrivalSongCount);
        Assert.Equal(100u, dan.SoulGaugeTotal);
        Assert.Equal(3, dan.DanStageScoreData.Count);
        Assert.Contains(dan.DanStageScoreData, row => row.StageIndex == 0 && row.SongNumber == 101 && row.HighScore == 326090);

        var normalBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.Null(normalBest);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniRejectsInvalidDanResultButKeepsNormalPlaySave()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 3,
                AryStageInfoes =
                [
                    new() { SongNo = 101, Level = 1, PlayScore = 123, PlayDan = 1 }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.DanScoreDataGreen.ToListAsync());
        Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniUnknownPlayDanKeepsNormalPlaySave()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 1,
                AryStageInfoes =
                [
                    new() { SongNo = 101, Level = 1, PlayScore = 123, PlayDan = 999 }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.DanScoreDataGreen.ToListAsync());
        Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniDuplicateSongsUseStageIndexRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 1,
                AryStageInfoes =
                [
                    new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 1 },
                    new() { SongNo = 101, Level = 1, PlayScore = 200, PlayDan = 1 }
                ]
            }),
            CancellationToken.None);

        var stages = await fixture.Context.DanStageScoreDataGreen
            .Where(row => row.Baid == 1 && row.DanId == 1)
            .OrderBy(row => row.StageIndex)
            .ToListAsync();

        Assert.Equal(2, stages.Count);
        Assert.Equal(0u, stages[0].StageIndex);
        Assert.Equal(1u, stages[1].StageIndex);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniNormalClearUpdatesFlagsAndDisplayDan()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 1,
                AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 1 }]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(1u, save!.GotDanMax);
        Assert.Equal(2u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.NormalClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
    }

    [Theory]
    [InlineData(1u, 101u)]
    [InlineData(101u, 104u)]
    public async Task UpdatePlayResult_Green_DaniClearEquipsSpecialDanCostume(uint danId, uint songNo)
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.Costume1 = 7;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 1,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 7
                },
                AryStageInfoes = [new() { SongNo = songNo, Level = 1, PlayScore = 100, PlayDan = danId }]
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(reloaded);
        Assert.Equal(36u, reloaded!.Costume1);
        Assert.True(BitIsSet(reloaded.CostumeFlg1, 36));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniClearKeepsCostumeWhenAutoCostumeOff()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.IsAutoCostumeOn = false;
        save.Costume1 = 7;
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 1,
                AryCurrentCostume = new CommonPlayResultData.CostumeData
                {
                    Costume1 = 36
                },
                AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 1 }]
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(reloaded);
        Assert.Equal(7u, reloaded!.Costume1);
        Assert.False(BitIsSet(reloaded.CostumeFlg1, 36));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniSkippedNormalClearAdvancesFromClearedDan()
    {
        var catalog = new GreenHandlerFixture.TestGreenCatalog(
            taikojukuFileOrder:
            [
                TestDanPack(1),
                TestDanPack(5)
            ]);
        await using var fixture = await GreenHandlerFixture.CreateAsync(catalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 1,
                AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 5 }]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(5u, save!.GotDanMax);
        Assert.Equal(6u, save.DispTaikojukuDan);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniFailedRetryAfterExistingNormalClearDoesNotAdvanceFromClearedDan()
    {
        var catalog = new GreenHandlerFixture.TestGreenCatalog(
            taikojukuFileOrder:
            [
                TestDanPack(5)
            ]);
        await using var fixture = await GreenHandlerFixture.CreateAsync(catalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 1,
                AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 5 }]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(6u, save!.DispTaikojukuDan);

        save.DispTaikojukuDan = 7;
        await fixture.Context.SaveChangesAsync();

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 0,
                AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 5 }]
            }),
            CancellationToken.None);

        Assert.Equal(5u, save.GotDanMax);
        Assert.Equal(7u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.NormalClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 4));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniLastNormalClearCapsDisplayDan()
    {
        var catalog = new GreenHandlerFixture.TestGreenCatalog(
            taikojukuFileOrder:
            [
                TestDanPack(25)
            ]);
        await using var fixture = await GreenHandlerFixture.CreateAsync(catalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 2,
                AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 25 }]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(25u, save!.GotDanMax);
        Assert.Equal(25u, save.DispTaikojukuDan);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniExtraClearUpdatesExtraFlagsOnly()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 2,
                AryStageInfoes = [new() { SongNo = 104, Level = 2, PlayScore = 100, PlayDan = 101 }]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(0u, save!.GotDanMax);
        Assert.Equal(1u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(save.GotDanExtraFlg, 0));
        Assert.Equal(Ac15DanClearGrade.NotClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniFailedAttemptDoesNotAdvanceDisplayDan()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        await handler.Handle(Ac15PlayResultTestFactory.FromCommon(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 1,
                DanResult = 0,
                AryStageInfoes = [new() { SongNo = 101, Level = 1, PlayScore = 100, PlayDan = 1 }]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(0u, save!.GotDanMax);
        Assert.Equal(1u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.NotClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
    }

    [Fact]
    public async Task GetDanScore_Green_ReturnsSavedChallengeLevelRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 1,
            IsExtra = false,
            MedleyUniqueId = 20001,
            ClearGrade = Ac15DanClearGrade.NormalClear,
            ArrivalSongCount = 2,
            SoulGaugeTotal = 150,
            ComboCountTotal = 300,
            DanStageScoreData =
            [
                new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 0, SongNumber = 101, PlayScore = 1000, HighScore = 1000, GoodCount = 10, OkCount = 2, BadCount = 1, DrumrollCount = 4, TotalHitCount = 13, ComboCount = 12 },
                new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 1, SongNumber = 102, PlayScore = 2000, HighScore = 2000, GoodCount = 20, OkCount = 3, BadCount = 0, DrumrollCount = 5, TotalHitCount = 23, ComboCount = 22 }
            ]
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new GetDanScoreQueryHandler(
            NullLogger<GetDanScoreQueryHandler>.Instance,
            fixture.Context,
            fixture.Catalog);

        var response = await handler.Handle(new GetDanScoreQuery(1, GameEra.Green, 0, [1]), CancellationToken.None);

        var dan = Assert.Single(response.AryDanScoreDatas);
        Assert.Equal(1u, dan.DanId);
        Assert.Equal(2u, dan.ArrivalSongCnt);
        Assert.Equal(150u, dan.SoulGaugeTotal);
        Assert.Equal(300u, dan.ComboCntTotal);
        Assert.Equal(2, dan.AryDanScoreDataStages.Count);
        Assert.Equal(1000u, dan.AryDanScoreDataStages[0].HighScore);
        Assert.Equal(2000u, dan.AryDanScoreDataStages[1].HighScore);
    }

    [Fact]
    public async Task GetSelfBest_Green_ReturnsSavedBest()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            BestScore = 765432,
            BestCrown = CrownType.Gold
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);

        var response = await handler.Handle(new GetSelfBestQuery(1, GameEra.Green, 1, [101]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 765432);
    }

    [Fact]
    public async Task GetSelfBest_Green_ReturnsNormalAndShinSavedBests()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataGreen.AddRange(
            new SongBestDatumGreen
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Easy,
                IsShin = false,
                BestScore = 229170,
                BestCrown = CrownType.Clear
            },
            new SongBestDatumGreen
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Easy,
                IsShin = true,
                BestScore = 897650,
                BestCrown = CrownType.Clear
            });
        await fixture.Context.SaveChangesAsync();

        var handler = new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance);

        var response = await handler.Handle(new GetSelfBestQuery(1, GameEra.Green, 1, [101, 102]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal([101u, 102u], response.ArySelfbestScores.Select(row => row.SongNo).ToArray());
        Assert.Equal([101u, 102u], response.AryShinSelfbestScores.Select(row => row.SongNo).ToArray());
        Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 229170);
        Assert.Contains(response.AryShinSelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 897650);
        Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 102 && row.SelfBestScore == 0);
        Assert.Contains(response.AryShinSelfbestScores, row => row.SongNo == 102 && row.SelfBestScore == 0);
    }

    [Fact]
    public void BuildGreenCrownResponseBody_EmptyRowsProduceAllZeroInflatedBody()
    {
        var packed = GreenCrownResponseBuilder.BuildInflatedBody([], new GreenHandlerFixture.TestGreenCatalog());

        Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, packed.Length);
        Assert.All(packed, value => Assert.Equal(0, value));
    }

    [Fact]
    public void BuildGreenCrownResponseBody_PacksSavedBestRows()
    {
        var rows = new[]
        {
            new SongBestDatumGreen { SongId = 101, Difficulty = Difficulty.Easy, BestCrown = CrownType.Clear },
            new SongBestDatumGreen { SongId = 101, Difficulty = Difficulty.Normal, BestCrown = CrownType.Gold }
        };

        var packed = GreenCrownResponseBuilder.BuildInflatedBody(rows, new GreenHandlerFixture.TestGreenCatalog());

        Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, packed.Length);
        Assert.Equal(0, ReadTenBitValue(packed, 0));
        Assert.Equal(0b0000_1110, ReadTenBitValue(packed, 101));
    }

    [Fact]
    public void BuildGreenCrownResponseBody_EncodesDondafulAsFullComboForGreen()
    {
        var rows = new[]
        {
            new SongBestDatumGreen { SongId = 101, Difficulty = Difficulty.Hard, BestCrown = CrownType.Dondaful }
        };

        var packed = GreenCrownResponseBuilder.BuildInflatedBody(rows, new GreenHandlerFixture.TestGreenCatalog());

        Assert.Equal(0b00_00_11_00_00, ReadTenBitValue(packed, 101));
    }

    [Fact]
    public async Task CrownsData_Green_UsesGreenSongNoAsCrownIndex()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog(
            musicInfoFileOrder:
            [
            new GreenMusicInfoEntry { SongNo = 463, MusicId = "class-id-729-song", FileOrder = 0 },
            new GreenMusicInfoEntry { SongNo = 729, MusicId = "lemon", FileOrder = 1 }
            ]));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 729,
            Difficulty = Difficulty.Easy,
            BestScore = 229170,
            BestCrown = CrownType.Clear
        });
        await fixture.Context.SaveChangesAsync();

        var controller = new CrownsDataController(fixture.Context, fixture.Catalog)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddLogging()
                        .BuildServiceProvider()
                }
            }
        };

        var result = await controller.CrownsData(new CrownsDataRequest
        {
            Baid = 1,
            ChassisId = "chassis",
            ShopId = "shop"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CrownsDataResponse>(ok.Value);
        var inflated = InflateGzip(response.HashCrownFlg);

        Assert.Equal(0, ReadTenBitValue(inflated, 1));
        Assert.Equal(0b0000_0010, ReadTenBitValue(inflated, 729));
        Assert.Equal(0, ReadTenBitValue(inflated, 463));
    }

    [Fact]
    public async Task CrownsData_Green_EmptyBestRowsReturnsAllZeroCrownTable()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var controller = new CrownsDataController(fixture.Context, fixture.Catalog)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddLogging()
                        .BuildServiceProvider()
                }
            }
        };

        var result = await controller.CrownsData(new CrownsDataRequest
        {
            Baid = 1,
            ChassisId = "chassis",
            ShopId = "shop"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CrownsDataResponse>(ok.Value);
        var inflated = InflateGzip(response.HashCrownFlg);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)123, response.SongHashVer);
        Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, inflated.Length);
        Assert.All(inflated, value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task CrownsData_Green_IncludesShinBestRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = true,
            BestScore = 897650,
            BestCrown = CrownType.Gold
        });
        await fixture.Context.SaveChangesAsync();

        var controller = new CrownsDataController(fixture.Context, fixture.Catalog)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddLogging()
                        .BuildServiceProvider()
                }
            }
        };

        var result = await controller.CrownsData(new CrownsDataRequest
        {
            Baid = 1,
            ChassisId = "chassis",
            ShopId = "shop"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CrownsDataResponse>(ok.Value);
        var inflated = InflateGzip(response.HashCrownFlg);

        Assert.Equal(0, ReadTenBitValue(inflated, 0));
        Assert.Equal(0b0000_1100, ReadTenBitValue(inflated, 101));
    }

    private static ushort ReadTenBitValue(byte[] packed, int songNo)
    {
        var value = 0;
        var bitOffset = songNo * 10;

        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= 1 << bit;
            }
        }

        return (ushort)value;
    }

    private static bool BitIsSet(byte[] source, uint id)
    {
        return (source[id >> 3] & (1 << ((int)id & 7))) != 0;
    }

    private static GreenTaikojukuEntry TestDanPack(uint danId)
        => new()
        {
            UniqueId = 20000 + danId,
            ChallengeLevel = danId,
            Songs =
            [
                new() { SongNo = 101, Level = 0 }
            ]
        };

    private static CommonPlayResultData.StageData PlainStage(uint songNo, bool isFavorite = false)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            PlayResult = 1,
            PlayScore = 100000,
            GoodCnt = 1,
            OkCnt = 0,
            NgCnt = 0,
            PoundCnt = 0,
            ComboCnt = 1,
            HitCnt = 1,
            OptionFlg = [0],
            ToneFlg = [0],
            MusicCateg = 0,
            IsFavorite = isFavorite,
            IsRecent = false
        };

    private static byte[] InflateGzip(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }
}
