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

        Assert.Empty(response.AryLegaltermsDatas);
        Assert.Empty(wire.AryLegaltermsDatas);
        Assert.False(response.IsBattleplay);
        Assert.Equal(new byte[BlueProtocolBytes.BattleStageFlagBytes], response.ReleaseBattleStageFlg);
        Assert.Equal(new byte[BlueProtocolBytes.BattleSpecialFlagBytes], response.ReleaseBattleSpecialFlg);
        Assert.Equal(0u, response.BattleBondsLvCap);
        Assert.True(wire.ShouldSerializeIsBattleplay());
        Assert.False(wire.IsBattleplay);
        Assert.True(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.Equal(new byte[BlueProtocolBytes.BattleStageFlagBytes], wire.ReleaseBattleStageFlg);
        Assert.True(wire.ShouldSerializeReleaseBattleSpecialFlg());
        Assert.Equal(new byte[BlueProtocolBytes.BattleSpecialFlagBytes], wire.ReleaseBattleSpecialFlg);
        Assert.True(wire.ShouldSerializeBattleBondsLvCap());
        Assert.Equal(0u, wire.BattleBondsLvCap);
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

        Assert.Contains(response.AryTelopDatas, row => row.InfoId == 7 && row.VerupNo == 4);
        Assert.Contains(response.AryEventFolderDatas, row => row.InfoId == 3 && row.VerupNo == 9);
        Assert.Contains(response.AryTaikojukuDatas, row => row.InfoId == 1);
    }

    [Fact]
    public async Task InitialData_BlueTaikojukuVerupNoUsesCatalogValue()
    {
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(
            taikojukuFileOrder:
            [
                new BlueTaikojukuEntry
                {
                    UniqueId = 20001,
                    ChallengeLevel = 1,
                    VerupNo = 7
                }
            ]);
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);

        var row = Assert.Single(response.AryTaikojukuDatas, data => data.InfoId == 1);
        Assert.Equal(7u, row.VerupNo);
    }

    [Fact]
    public async Task InitialData_Blue_AdvertisesParsedBattleStageAvailabilityAndSpecials()
    {
        var battleCatalog = new BlueBattleCatalog
        {
            IsRawDataAvailable = true,
            EnablesBattleAdvertisement = true,
            ReleaseBattleStageIds = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 33],
            ReleaseBattleSpecialIds = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15],
            BattleBondsLvCap = 65,
            Files = []
        };
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(battleCatalog: battleCatalog);
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);
        var wire = InitialDataMappers.Map(response);

        Assert.True(response.IsBattleplay);
        Assert.Equal(
            [0xFE, 0x07, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00],
            response.ReleaseBattleStageFlg);
        Assert.Equal(
            [0xFE, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01],
            response.ReleaseBattleSpecialFlg);
        Assert.Equal(65u, response.BattleBondsLvCap);
        Assert.True(wire.ShouldSerializeIsBattleplay());
        Assert.True(wire.IsBattleplay);
        Assert.True(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.Equal(response.ReleaseBattleStageFlg, wire.ReleaseBattleStageFlg);
        Assert.True(wire.ShouldSerializeReleaseBattleSpecialFlg());
        Assert.Equal(response.ReleaseBattleSpecialFlg, wire.ReleaseBattleSpecialFlg);
        Assert.True(wire.ShouldSerializeBattleBondsLvCap());
        Assert.Equal(65u, wire.BattleBondsLvCap);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
