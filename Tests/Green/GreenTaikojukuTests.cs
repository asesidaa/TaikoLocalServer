namespace TaikoLocalServer.Tests.Green;

public sealed class GreenTaikojukuTests
{
    [Fact]
    public async Task GetTaikojuku_ReturnsRequestedUniqueIdPack()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([20001]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.NotEmpty(response.Packs);
    }

    [Fact]
    public async Task GetTaikojuku_ReturnsDeterministicFallbackWhenNoPackMatches()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([999999]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.NotEmpty(response.Packs);
        Assert.NotEmpty(response.Packs[0].Songs);
    }
}
