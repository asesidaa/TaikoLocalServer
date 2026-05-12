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
