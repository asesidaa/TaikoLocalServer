using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenEventFolderProtocolTests
{
    [Fact]
    public async Task InitialData_AdvertisesGreenEventFoldersByProtocolId()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Contains(response.AryGreenEventFolderDatas, row => row.InfoId == 1 && row.VerupNo == 0);
        Assert.Contains(response.AryGreenEventFolderDatas, row => row.InfoId == 11 && row.VerupNo == 3);
        Assert.Equal(2, response.AryGreenEventFolderDatas.Count);
    }

    [Fact]
    public async Task InitialData_EmptyGreenEventFolderCatalogProducesNoRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Green), CancellationToken.None);

        Assert.Empty(response.AryGreenEventFolderDatas);
    }

    private static GreenHandlerFixture.TestGreenCatalog CreateCatalogWithFolders()
        => new(eventFolders: new Dictionary<uint, EventFolderData>
        {
            [1] = new()
            {
                FolderId = 1,
                VerupNo = 0,
                SongNoes = [101, 102]
            },
            [11] = new()
            {
                FolderId = 11,
                VerupNo = 3,
                SongNoes = [103]
            }
        });
}
