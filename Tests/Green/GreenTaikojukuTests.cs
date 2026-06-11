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

        var response = await handler.Handle(new GetTaikojukuQuery(GameEra.Green, [1]), CancellationToken.None);

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

        var response = await handler.Handle(new GetTaikojukuQuery(GameEra.Green, [20001]), CancellationToken.None);

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
            new GetTaikojukuQuery(GameEra.Green, Enumerable.Range(101, 25).Select(value => (uint)value).ToArray()),
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

        var response = await handler.Handle(new GetTaikojukuQuery(GameEra.Green, [101]), CancellationToken.None);
        var wire = TaikojukuMappers.Map(response);

        Assert.Single(wire.AryJukupackDatas);
        Assert.DoesNotContain(wire.AryJukupackDatas, pack => pack.GetDan is 101 or 20026);
        Assert.All(wire.AryJukupackDatas, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    [Fact]
    public async Task GetTaikojuku_ReturnsDeterministicFallbackWhenNoPackMatches()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery(GameEra.Green, [5]), CancellationToken.None);

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

        Assert.NotEmpty(response.AryTaikojukuDatas);
        Assert.Contains(response.AryTaikojukuDatas, data => data.InfoId == 1);
        Assert.DoesNotContain(response.AryTaikojukuDatas, data => data.InfoId == 20001);
        Assert.All(response.AryTaikojukuDatas, data => Assert.InRange(data.InfoId, 1u, 25u));
    }

    [Fact]
    public async Task InitialData_GreenTaikojukuVerupNoUsesCatalogValue()
    {
        var greenCatalog = new GreenHandlerFixture.TestGreenCatalog(
            taikojukuFileOrder:
            [
                new GreenTaikojukuEntry
                {
                    UniqueId = 20001,
                    ChallengeLevel = 1,
                    VerupNo = 7
                }
            ]);
        await using var fixture = await GreenHandlerFixture.CreateAsync(greenCatalog);
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        var row = Assert.Single(response.AryTaikojukuDatas, data => data.InfoId == 1);
        Assert.Equal(7u, row.VerupNo);
    }

    [Fact]
    public async Task InitialData_DoesNotAdvertiseEmptyGreenItemShop()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.False(response.IsItemshop);
    }
}
