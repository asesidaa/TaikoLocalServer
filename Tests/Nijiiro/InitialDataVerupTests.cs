using System.Collections.Immutable;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Domain;
using TaikoLocalServer.Tests.Green;

namespace TaikoLocalServer.Tests.Nijiiro;

// initialdatacheck's generic master versions. The 39.06 cabinet resets its mask for any generic master it gets no
// version for (the cabinet updater), and the next card scan then crashes ANDing the player's costume flags with an
// empty mask. Seen when these were dropped from the reply.
public sealed class InitialDataVerupTests
{
    [Fact]
    public async Task InitialData_Nijiiro_ListsTheGenericMastersAtVersionOne()
    {
        var catalog = new FileGameDataCatalog([new EventFolderLookupSharedTests.TestNijiiroCatalog(ImmutableDictionary<uint, EventFolderData>.Empty, shopFolderVerup: 7)]);
        var handler = new GetInitialDataQueryHandler(catalog, new NullCabinetRegistry(),
            NullLogger<GetInitialDataQueryHandler>.Instance, Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Nijiiro), CancellationToken.None);

        var versions = response.AryVerupNoData1s.ToDictionary(v => v.MasterType, v => v.VerupNo);
        foreach (var type in new uint[] { 2, 3, 4, 5, 6, 7, 8, 13, 15, 24, 25, 26, 27, 28, 29, 30, 31 })
            Assert.Equal(1u, versions.GetValueOrDefault(type));
        Assert.Equal(7u, versions[DomainConstants.ShopVerupMasterType]);
    }
}
