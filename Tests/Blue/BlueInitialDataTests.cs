using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueInitialDataTests
{
    [Fact]
    public async Task InitialData_Blue_UnlocksAllCatalogSongsAndLeavesLegalTermsEmpty()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);
        var wire = InitialDataMappers.Map(response);

        Assert.Equal(1u, response.Result);
        Assert.Equal(BlueProtocolBytes.SongFlagBytes, response.DefaultSongFlg.Length);
        foreach (var song in fixture.Catalog.Blue().MusicInfoFileOrder)
        {
            Assert.True(BitIsSet(response.DefaultSongFlg, song.SongNo), $"Expected song {song.SongNo} to be unlocked.");
        }

        Assert.Empty(response.AryBlueLegaltermsDatas);
        Assert.Empty(wire.AryLegaltermsDatas);
        Assert.False(wire.ShouldSerializeIsBattleplay());
        Assert.False(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.False(wire.ShouldSerializeReleaseBattleSpecialFlg());
        Assert.False(wire.ShouldSerializeBattleBondsLvCap());
    }

    [Fact]
    public async Task InitialData_Blue_MapsCatalogInformationArrays()
    {
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(
            eventFolders: new Dictionary<uint, EventFolderData>
            {
                [3] = new() { FolderId = 3, VerupNo = 9, SongNoes = [101, 102] }
            },
            telops: new Dictionary<uint, BlueTelopEntry>
            {
                [7] = new() { TelopId = 7, VerupNo = 4 }
            });
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);

        Assert.Contains(response.AryBlueTelopDatas, row => row.InfoId == 7 && row.VerupNo == 4);
        Assert.Contains(response.AryBlueEventFolderDatas, row => row.InfoId == 3 && row.VerupNo == 9);
        Assert.Contains(response.AryBlueTaikojukuDatas, row => row.InfoId == 1);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
