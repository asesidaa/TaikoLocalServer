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

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        var result = await handler.Handle(new UpdatePlayResultCommand(
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
    public async Task UpdatePlayResult_Green_RejectsUnknownStageMode()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        Assert.Equal(0u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_RejectsOutOfCatalogSongNo()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        Assert.Equal((uint)0, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.GreenFavoriteSongs.ToListAsync());
        Assert.Empty(await fixture.Context.GreenRecentSongs.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_RejectsStageLevelOutsideOneThroughFive()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        Assert.Equal((uint)0, result);
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

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                GetDonmedal = 1
            }),
            CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.Equal((uint)0, result);
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

        var result = await handler.Handle(new UpdatePlayResultCommand(
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
    public async Task UpdatePlayResult_Green_RejectsOutOfRangeRewardIds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        Assert.Equal(0u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
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
            ClearGrade = GreenDanClearGrade.GoldClear,
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

        Assert.Equal(GreenDanClearGrade.GoldClear, saved.ClearGrade);
        var stage = Assert.Single(saved.DanStageScoreData);
        Assert.Equal(0u, stage.StageIndex);
        Assert.Equal(790u, stage.SongNumber);
    }

    [Fact]
    public void GreenDanHelpers_ClassifiesNormalAndExtraDanIds()
    {
        Assert.True(GreenDanHelpers.IsNormalDanId(1));
        Assert.True(GreenDanHelpers.IsNormalDanId(25));
        Assert.False(GreenDanHelpers.IsNormalDanId(26));

        Assert.True(GreenDanHelpers.IsExtraDanId(101));
        Assert.True(GreenDanHelpers.IsExtraDanId(128));
        Assert.False(GreenDanHelpers.IsExtraDanId(100));
    }

    [Fact]
    public void GreenDanHelpers_PacksTwoBitClearGrades()
    {
        var flags = GreenDanHelpers.SetPackedGrade(new byte[GreenProtocolBytes.DanFlagBytes], 0, GreenDanClearGrade.NormalClear);
        flags = GreenDanHelpers.SetPackedGrade(flags, 1, GreenDanClearGrade.GoldClear);

        Assert.Equal(GreenDanClearGrade.NormalClear, GreenDanHelpers.GetPackedGrade(flags, 0));
        Assert.Equal(GreenDanClearGrade.GoldClear, GreenDanHelpers.GetPackedGrade(flags, 1));
        Assert.Equal(GreenDanClearGrade.NotClear, GreenDanHelpers.GetPackedGrade(flags, 2));
    }

    [Fact]
    public void GreenDanHelpers_EncodesGoldClearAsThreeForClientFlags()
    {
        var flags = GreenDanHelpers.SetPackedGrade(new byte[GreenProtocolBytes.DanFlagBytes], 0, GreenDanClearGrade.GoldClear);

        Assert.Equal(0b0000_0011, flags[0] & 0b11);
        Assert.Equal(GreenDanClearGrade.GoldClear, GreenDanHelpers.GetPackedGrade(flags, 0));
    }

    [Fact]
    public void GreenDanHelpers_ComputesNextUnclearedNormalDan()
    {
        var grades = new Dictionary<uint, GreenDanClearGrade>
        {
            [1] = GreenDanClearGrade.NormalClear,
            [2] = GreenDanClearGrade.GoldClear
        };

        Assert.Equal(3u, GreenDanHelpers.GetNextUnclearedNormalDan(grades));

        for (uint dan = 3; dan <= 25; dan++)
        {
            grades[dan] = GreenDanClearGrade.NormalClear;
        }

        Assert.Equal(25u, GreenDanHelpers.GetNextUnclearedNormalDan(grades));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_DaniPlaySavesDanDataAndNormalBest()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
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
        Assert.Equal(GreenDanClearGrade.GoldClear, dan.ClearGrade);
        Assert.Equal(3u, dan.ArrivalSongCount);
        Assert.Equal(3, dan.DanStageScoreData.Count);
        Assert.Contains(dan.DanStageScoreData, row => row.StageIndex == 0 && row.SongNumber == 101 && row.HighScore == 326090);

        var normalBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(normalBest);
        Assert.Equal(326090u, normalBest!.BestScore);
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

        var result = await handler.Handle(new UpdatePlayResultCommand(
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

        await handler.Handle(new UpdatePlayResultCommand(
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

        await handler.Handle(new UpdatePlayResultCommand(
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
        Assert.Equal(GreenDanClearGrade.NormalClear, GreenDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
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

        await handler.Handle(new UpdatePlayResultCommand(
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
        Assert.Equal(GreenDanClearGrade.GoldClear, GreenDanHelpers.GetPackedGrade(save.GotDanExtraFlg, 0));
        Assert.Equal(GreenDanClearGrade.NotClear, GreenDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
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

        await handler.Handle(new UpdatePlayResultCommand(
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
        Assert.Equal(GreenDanClearGrade.NotClear, GreenDanHelpers.GetPackedGrade(save.GotDanFlg, 0));
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
            ClearGrade = GreenDanClearGrade.NormalClear,
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

    private static byte[] InflateGzip(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }
}
