using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenPlayResultHandlerTests
{
    [Theory]
    [InlineData(0, Difficulty.Easy)]
    [InlineData(1, Difficulty.Normal)]
    [InlineData(2, Difficulty.Hard)]
    [InlineData(3, Difficulty.Oni)]
    [InlineData(4, Difficulty.UraOni)]
    public void MapDifficulty_UsesGreenCourseOrder(uint level, Difficulty expected)
    {
        Assert.Equal(expected, GreenPlayResultMapping.MapDifficulty(level));
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
                        Level = 0,
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
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Easy);
        Assert.NotNull(best);
        Assert.Equal((uint)765432, best!.BestScore);
        Assert.Equal(CrownType.Gold, best.BestCrown);
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
                        Level = 0,
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
    public async Task UpdatePlayResult_Green_RejectsStageLevelOutsideZeroThroughFour()
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
                        Level = 5,
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
                        Level = 0,
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
                        Level = 0,
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

        var response = await handler.Handle(new GetSelfBestQuery(1, GameEra.Green, 0, [101]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 765432);
    }

    [Fact]
    public async Task GetSelfBest_Green_ReturnsParallelZeroShinRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            BestScore = 765432,
            BestCrown = CrownType.Gold
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
        Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 765432);
        Assert.All(response.AryShinSelfbestScores, row =>
        {
            Assert.Equal((uint)0, row.SelfBestScore);
            Assert.Equal((uint)0, row.UraBestScore);
        });
    }

    [Fact]
    public void BuildGreenCrownResponseBody_EmptyRowsProduceAllZeroInflatedBody()
    {
        var packed = GreenCrownResponseBuilder.BuildInflatedBody([]);

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

        var packed = GreenCrownResponseBuilder.BuildInflatedBody(rows);

        Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, packed.Length);
        Assert.Equal(0b0000_1001, ReadTenBitValue(packed, 101));
    }

    [Fact]
    public void BuildGreenCrownResponseBody_EncodesDondafulAsFullComboForGreen()
    {
        var rows = new[]
        {
            new SongBestDatumGreen { SongId = 101, Difficulty = Difficulty.Hard, BestCrown = CrownType.Dondaful }
        };

        var packed = GreenCrownResponseBuilder.BuildInflatedBody(rows);

        Assert.Equal(0b00_00_10_00_00, ReadTenBitValue(packed, 101));
    }

    [Fact]
    public void BuildGreenCrownResponseBody_UsesSongIdAsCrownIndex()
    {
        var rows = new[]
        {
            new SongBestDatumGreen { SongId = 873, Difficulty = Difficulty.Easy, BestCrown = CrownType.Clear }
        };

        var packed = GreenCrownResponseBuilder.BuildInflatedBody(rows);

        Assert.Equal(0, ReadTenBitValue(packed, 0));
        Assert.Equal(0b0000_0001, ReadTenBitValue(packed, 873));
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
        var inflated = InflateZlib(response.HashCrownFlg);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)123, response.SongHashVer);
        Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, inflated.Length);
        Assert.All(inflated, value => Assert.Equal(0, value));
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

    private static byte[] InflateZlib(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }
}
