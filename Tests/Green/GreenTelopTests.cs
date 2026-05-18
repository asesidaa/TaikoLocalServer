using System.Text;
using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenTelopTests
{
    [Fact]
    public async Task Loader_ReturnsEmptyWhenFileMissing()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");

        var telops = await GreenTelopLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Empty(telops);
    }

    [Fact]
    public async Task Loader_ReadsCamelCaseFieldsAndIgnoresZeroIdRows()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                [
                  { "telopId": 0, "verupNo": 9, "telop": "should be skipped" },
                  { "telopId": 1, "verupNo": 1, "startDatetime": "20240101000000", "endDatetime": "20991231235959", "telop": "Hello" },
                  { "telopId": 2, "verupNo": 2, "telop": "No dates" }
                ]
                """, Encoding.UTF8);

            var telops = await GreenTelopLoader.LoadFromFileAsync(path, CancellationToken.None);

            Assert.Equal(2, telops.Count);
            Assert.True(telops.TryGetValue(1, out var first));
            Assert.Equal(1u, first!.VerupNo);
            Assert.Equal("20240101000000", first.StartDatetime);
            Assert.Equal("20991231235959", first.EndDatetime);
            Assert.Equal("Hello", first.Message);
            Assert.True(telops.TryGetValue(2, out var second));
            Assert.Equal(string.Empty, second!.StartDatetime);
            Assert.Equal("No dates", second.Message);
            Assert.DoesNotContain(0u, telops.Keys);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task InitialData_AdvertisesTelopsByIdInOrder()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog
        {
            Telops = new Dictionary<uint, GreenTelopEntry>
            {
                [2] = new() { TelopId = 2, VerupNo = 5, Message = "Second" },
                [1] = new() { TelopId = 1, VerupNo = 3, Message = "First" }
            }
        });

        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Collection(
            response.AryGreenTelopDatas,
            row => { Assert.Equal(1u, row.InfoId); Assert.Equal(3u, row.VerupNo); },
            row => { Assert.Equal(2u, row.InfoId); Assert.Equal(5u, row.VerupNo); });
    }

    [Fact]
    public async Task InitialData_EmptyTelopCatalogProducesNoRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();

        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Empty(response.AryGreenTelopDatas);
    }

    [Fact]
    public async Task GetTelop_ReturnsCatalogEntryWithFourteenCharDatetimes()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog
        {
            Telops = new Dictionary<uint, GreenTelopEntry>
            {
                [7] = new()
                {
                    TelopId = 7,
                    VerupNo = 4,
                    StartDatetime = "20240101000000",
                    EndDatetime = "20991231235959",
                    Message = "Hello Green"
                }
            }
        });

        var handler = new GetTelopQueryHandler(fixture.Catalog);

        var common = await handler.Handle(new GetTelopQuery(GameEra.Green, 7), CancellationToken.None);
        var wire = GetTelopMappers.Map(common);

        Assert.Equal(1u, wire.Result);
        Assert.Equal(4u, wire.VerupNo);
        Assert.Equal("20240101000000", wire.StartDatetime);
        Assert.Equal("20991231235959", wire.EndDatetime);
        Assert.Equal("Hello Green", wire.Telop);
        Assert.Equal(14, wire.StartDatetime.Length);
        Assert.Equal(14, wire.EndDatetime.Length);
    }

    [Fact]
    public async Task GetTelop_UnknownIdReturnsSuccessWithOmittedOptionalFields()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTelopQueryHandler(fixture.Catalog);

        var common = await handler.Handle(new GetTelopQuery(GameEra.Green, 99), CancellationToken.None);
        var wire = GetTelopMappers.Map(common);

        Assert.Equal(1u, wire.Result);
        Assert.False(wire.ShouldSerializeVerupNo());
        Assert.False(wire.ShouldSerializeStartDatetime());
        Assert.False(wire.ShouldSerializeEndDatetime());
        Assert.False(wire.ShouldSerializeTelop());
    }

    [Fact]
    public void Mapper_OmitsOptionalFieldsWhenCommonResponseHasNoValues()
    {
        var wire = GetTelopMappers.Map(new CommonGetTelopResponse { Result = 1 });

        Assert.Equal(1u, wire.Result);
        Assert.False(wire.ShouldSerializeVerupNo());
        Assert.False(wire.ShouldSerializeStartDatetime());
        Assert.False(wire.ShouldSerializeEndDatetime());
        Assert.False(wire.ShouldSerializeTelop());
    }

    [Fact]
    public void DateTimeFormat_MatchesExistingConstant()
    {
        Assert.Equal("yyyyMMddHHmmss", Constants.DateTimeFormat);
    }
}
