using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
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

        Assert.Contains(response.AryEventFolderDatas, row => row.InfoId == 1 && row.VerupNo == 0);
        Assert.Contains(response.AryEventFolderDatas, row => row.InfoId == 11 && row.VerupNo == 3);
        Assert.Equal(2, response.AryEventFolderDatas.Count);
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

        Assert.Empty(response.AryEventFolderDatas);
    }

    [Fact]
    public async Task GetFolder_GreenReturnsKnownRequestedFoldersAndOmitsUnknownIds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetFolderQueryHandler(
            NullLogger<GetFolderQueryHandler>.Instance,
            fixture.Catalog);

        var response = await handler.Handle(new GetFolderQuery(GameEra.Green, [11, 99, 1]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Collection(
            response.AryEventfolderDatas,
            row =>
            {
                Assert.Equal(11u, row.FolderId);
                Assert.Equal(3u, row.VerupNo);
                Assert.Equal(new uint[] { 103 }, row.SongNoes);
            },
            row =>
            {
                Assert.Equal(1u, row.FolderId);
                Assert.Equal(0u, row.VerupNo);
                Assert.Equal(new uint[] { 101, 102 }, row.SongNoes);
            });
    }

    [Fact]
    public async Task GetFolder_GreenMapperPopulatesWireRows()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CreateCatalogWithFolders());
        var handler = new GetFolderQueryHandler(
            NullLogger<GetFolderQueryHandler>.Instance,
            fixture.Catalog);

        var common = await handler.Handle(new GetFolderQuery(GameEra.Green, [1]), CancellationToken.None);
        var wire = FolderDataMappers.Map(common);

        Assert.Equal(1u, wire.Result);
        var row = Assert.Single(wire.AryEventfolderDatas);
        Assert.Equal(1u, row.FolderId);
        Assert.Equal(0u, row.VerupNo);
        Assert.Equal(new uint[] { 101, 102 }, row.SongNoes);
        Assert.True(row.ShouldSerializeFolderId());
        Assert.True(row.ShouldSerializeVerupNo());
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
