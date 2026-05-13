using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenTaikojukuTests
{
    [Fact]
    public async Task GetTaikojuku_ReturnsRequestedDanSlotPack()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([1]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.NotEmpty(response.Packs);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
        Assert.Contains(response.Packs, pack => pack.GetDan == 1);
    }

    [Fact]
    public async Task GetTaikojuku_DoesNotTreatUniqueIdAsDanSlot()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([20001]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Single(response.Packs);
        Assert.DoesNotContain(response.Packs, pack => pack.GetDan == 20001);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    [Fact]
    public async Task GetTaikojuku_AllInvalidRequestSlotsFallbackIsCappedToEleven()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetTaikojukuQuery(Enumerable.Range(101, 25).Select(value => (uint)value).ToArray()),
            CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal(11, response.Packs.Count);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    [Fact]
    public async Task GetTaikojuku_DoesNotSerializeOutOfRangeChallengeLevels()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([101]), CancellationToken.None);
        var wire = TaikojukuMappers.Map(response);

        Assert.Single(wire.AryJukupackDatas);
        Assert.DoesNotContain(wire.AryJukupackDatas, pack => pack.GetDan is 101 or 20026);
        Assert.All(wire.AryJukupackDatas, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    [Fact]
    public void TaikojukuMapper_DropsInvalidPackSlotsAndCapsSongsAtTen()
    {
        var response = TaikojukuMappers.Map(new CommonTaikojukuResponse
        {
            Result = 1,
            Packs =
            [
                new CommonTaikojukuResponse.Pack
                {
                    GetDan = 0,
                    Songs = [new CommonTaikojukuResponse.Song { SongNo = 101, Level = 0 }]
                },
                new CommonTaikojukuResponse.Pack
                {
                    GetDan = 1,
                    Songs =
                    [
                        new CommonTaikojukuResponse.Song { SongNo = 0, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 1024, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 101, Level = 5 },
                        new CommonTaikojukuResponse.Song { SongNo = 101, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 102, Level = 1 },
                        new CommonTaikojukuResponse.Song { SongNo = 103, Level = 2 },
                        new CommonTaikojukuResponse.Song { SongNo = 104, Level = 3 },
                        new CommonTaikojukuResponse.Song { SongNo = 105, Level = 4 },
                        new CommonTaikojukuResponse.Song { SongNo = 106, Level = 0 },
                        new CommonTaikojukuResponse.Song { SongNo = 107, Level = 1 },
                        new CommonTaikojukuResponse.Song { SongNo = 108, Level = 2 },
                        new CommonTaikojukuResponse.Song { SongNo = 109, Level = 3 },
                        new CommonTaikojukuResponse.Song { SongNo = 110, Level = 4 },
                        new CommonTaikojukuResponse.Song { SongNo = 111, Level = 0 }
                    ]
                }
            ]
        });

        var pack = Assert.Single(response.AryJukupackDatas);
        Assert.Equal((uint)1, pack.GetDan);
        Assert.Equal(10, pack.AryJukusongDatas.Count);
        Assert.DoesNotContain(pack.AryJukusongDatas, song => song.SongNo is 0 or 1024);
        Assert.All(pack.AryJukusongDatas, song => Assert.InRange(song.Level, 0u, 4u));
    }

    [Fact]
    public async Task GetTaikojuku_ReturnsDeterministicFallbackWhenNoPackMatches()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([5]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.Single(response.Packs);
        Assert.NotEmpty(response.Packs[0].Songs);
        Assert.Equal((uint)5, response.Packs[0].GetDan);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    [Fact]
    public async Task InitialData_GreenTaikojukuInfoIdsUseDanSlots()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.NotEmpty(response.AryGreenTaikojukuDatas);
        Assert.Contains(response.AryGreenTaikojukuDatas, data => data.InfoId == 1);
        Assert.DoesNotContain(response.AryGreenTaikojukuDatas, data => data.InfoId == 20001);
        Assert.All(response.AryGreenTaikojukuDatas, data => Assert.InRange(data.InfoId, 1u, 25u));
    }
}
