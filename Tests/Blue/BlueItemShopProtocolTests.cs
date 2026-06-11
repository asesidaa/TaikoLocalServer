using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
using TaikoLocalServer.Application.Catalog.Blue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueItemShopProtocolTests
{
    [Fact]
    public async Task InitialData_DisabledShopDoesNotAdvertiseItemShop()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);
        var wire = InitialDataMappers.Map(response);

        Assert.False(response.IsItemshop);
        Assert.Empty(response.AryItemShopDatas);
        Assert.False(wire.IsItemshop);
        Assert.Empty(wire.AryItemshopDatas);
    }

    [Fact]
    public async Task InitialData_MissingActiveSeasonDoesNotAdvertiseItemShop()
    {
        var catalog = new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 99,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = CreateSeason()
            }
        };
        await using var fixture = await BlueHandlerFixture.CreateAsync(
            new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: catalog));
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);
        var wire = InitialDataMappers.Map(response);

        Assert.False(response.IsItemshop);
        Assert.Empty(response.AryItemShopDatas);
        Assert.False(wire.IsItemshop);
        Assert.Empty(wire.AryItemshopDatas);
    }

    [Fact]
    public async Task InitialData_EmptyActiveSeasonDoesNotAdvertiseItemShop()
    {
        var catalog = new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = new() { SeasonId = 2, VerupNo = 20170404, Items = [] }
            }
        };
        await using var fixture = await BlueHandlerFixture.CreateAsync(
            new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: catalog));
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);
        var wire = InitialDataMappers.Map(response);

        Assert.False(response.IsItemshop);
        Assert.Empty(response.AryItemShopDatas);
        Assert.False(wire.IsItemshop);
        Assert.Empty(wire.AryItemshopDatas);
    }

    [Fact]
    public async Task InitialData_EnabledActiveSeasonAdvertisesMetadataAndClearsShopSongs()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateSongShopCatalog());
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Blue), CancellationToken.None);
        var wire = InitialDataMappers.Map(response);

        Assert.True(response.IsItemshop);
        var info = Assert.Single(response.AryItemShopDatas);
        Assert.Equal(2u, info.InfoId);
        Assert.Equal(20170404u, info.VerupNo);
        Assert.True(wire.IsItemshop);
        var wireInfo = Assert.Single(wire.AryItemshopDatas);
        Assert.Equal(2u, wireInfo.InfoId);
        Assert.Equal(20170404u, wireInfo.VerupNo);
        Assert.False(HasBit(response.DefaultSongFlg, 101));
        Assert.True(HasBit(response.DefaultSongFlg, 102));
        Assert.Equal(BlueProtocolBytes.SongFlagBytes, response.DefaultSongFlg.Length);
    }

    [Fact]
    public async Task GetItemShopInfo_DisabledShopReturnsSuccessWithoutRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(GameEra.Blue), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Empty(wire.AryItemshopDatas);
        Assert.Equal(0u, wire.SeasonId);
        Assert.Equal(0u, wire.VerupNo);
    }

    [Fact]
    public async Task GetItemShopInfo_InactiveShopReturnsSuccessWithoutRows()
    {
        var catalog = new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 99,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = CreateSeason()
            }
        };
        await using var fixture = await BlueHandlerFixture.CreateAsync(
            new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: catalog));
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(GameEra.Blue), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Empty(wire.AryItemshopDatas);
        Assert.Equal(0u, wire.SeasonId);
        Assert.Equal(0u, wire.VerupNo);
    }

    [Fact]
    public async Task GetItemShopInfo_EnabledShopReturnsActiveSeasonRowsOrderedByItemNo()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var response = await handler.Handle(new GetItemShopInfoQuery(GameEra.Blue), CancellationToken.None);
        var wire = ItemShopMappers.Map(response);

        Assert.Equal(1u, wire.Result);
        Assert.Equal(2u, wire.SeasonId);
        Assert.Equal(20170404u, wire.VerupNo);
        Assert.Equal("Blue Shop", wire.Telop);
        Assert.Equal("20181219070000", wire.StartDatetime);
        Assert.Equal("20190314020000", wire.EndDatetime);
        Assert.Equal(30u, wire.AfterstartDays);
        Assert.Equal(0u, wire.BeforecloseDays);
        Assert.Equal(2, wire.AryItemshopDatas.Count);
        Assert.Equal(1u, wire.AryItemshopDatas[0].ItemNo);
        Assert.Equal(3u, wire.AryItemshopDatas[0].ItemType);
        Assert.Equal(12u, wire.AryItemshopDatas[0].ItemId);
        Assert.Equal(1300u, wire.AryItemshopDatas[0].ItemPrice);
        Assert.Equal(2u, wire.AryItemshopDatas[1].ItemNo);
    }

    [Fact]
    public async Task GetItemShopInfo_UnsupportedEraThrows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var handler = new GetItemShopInfoQueryHandler(
            fixture.Catalog,
            NullLogger<GetItemShopInfoQueryHandler>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(new GetItemShopInfoQuery((GameEra)999), CancellationToken.None));

        Assert.Contains("Unsupported era", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BlueGetItemShopInfoMap_UsesBlueEra()
    {
        var query = ItemShopMappers.Map(new GetitemshopinfoRequest
        {
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123"
        });

        Assert.Equal(GameEra.Blue, query.Era);
    }

    [Fact]
    public void ItemPurchaseCommandMap_PreservesOmittedOptionalDetails()
    {
        var request = new ItempurchaseRequest
        {
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            Baid = 1,
            ItemNo = 0
        };

        var command = ItemShopMappers.Map(request);

        Assert.Equal(1u, command.Baid);
        Assert.Equal(GameEra.Blue, command.Era);
        Assert.Equal(0u, command.ItemNo);
        Assert.Null(command.ItemType);
        Assert.Null(command.ItemId);
        Assert.Null(command.ItemPrice);
    }

    [Fact]
    public void ItemPurchaseCommandMap_PreservesExplicitZeroOptionalDetails()
    {
        var request = new ItempurchaseRequest
        {
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            Baid = 1,
            ItemNo = 0,
            ItemType = 0,
            ItemId = 0,
            ItemPrice = 0
        };

        var command = ItemShopMappers.Map(request);

        Assert.Equal(0u, command.ItemType);
        Assert.Equal(0u, command.ItemId);
        Assert.Equal(0u, command.ItemPrice);
    }

    [Fact]
    public async Task BlueRewardExecution_ReturnsSuccessWithoutMutatingShopOrSaveState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, [4], BlueProtocolBytes.ToneFlagBytes);
        save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [12], BlueProtocolBytes.CostumeFlagBytes);
        fixture.Context.UserSaveDataBlue.Add(save);
        fixture.Context.BlueShopSeasonStates.Add(new BlueShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = 700,
            TotalUseDonmedal = 200,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        fixture.Context.BlueShopItemStates.Add(new BlueShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 3,
            ItemId = 12,
            ItemNo = 1,
            ItemPrice = 1300,
            Status = Ac15ShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        var unlockFieldsBefore = SnapshotUnlockFields(save);
        var controller = new RewardExecutionController
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

        var result = controller.RewardExecution(new RewardexecutionRequest
        {
            Baid = 1,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            ReleaseSongNoes = [101],
            GetToneNoes = [5],
            GetCostumeNo1s = [13]
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RewardexecutionResponse>(ok.Value);
        var season = await fixture.Context.BlueShopSeasonStates.FindAsync(1u, 2u);
        var reloaded = await fixture.Context.UserSaveDataBlue.FindAsync(1u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(700u, season!.TotalGetDonmedal);
        Assert.Equal(200u, season.TotalUseDonmedal);
        foreach (var (field, bytes) in unlockFieldsBefore)
        {
            Assert.Equal(bytes, GetUnlockField(reloaded!, field));
        }

        Assert.Single(await fixture.Context.BlueShopItemStates.ToListAsync());
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateShopCatalog()
    {
        return new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = CreateSeason()
            }
        });
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateSongShopCatalog()
    {
        return new BlueHandlerFixture.TestBlueCatalog(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = new()
                {
                    SeasonId = 2,
                    VerupNo = 20170404,
                    Items =
                    [
                        new BlueItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 1300 },
                        new BlueItemShopEntry { ItemNo = 2, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }
                    ]
                }
            }
        });
    }

    private static BlueItemShopSeason CreateSeason()
        => new()
        {
            SeasonId = 2,
            VerupNo = 20170404,
            Telop = "Blue Shop",
            StartDatetime = "20181219070000",
            EndDatetime = "20190314020000",
            AfterstartDays = 30,
            BeforecloseDays = 0,
            Items =
            [
                new BlueItemShopEntry { ItemNo = 2, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 7, Price = 1500 },
                new BlueItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }
            ]
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private static Dictionary<string, byte[]> SnapshotUnlockFields(UserSaveDataBlue saveData)
        => new()
        {
            [nameof(UserSaveDataBlue.ReleaseSongFlg)] = saveData.ReleaseSongFlg.ToArray(),
            [nameof(UserSaveDataBlue.ToneFlg)] = saveData.ToneFlg.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg1)] = saveData.CostumeFlg1.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg2)] = saveData.CostumeFlg2.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg3)] = saveData.CostumeFlg3.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg4)] = saveData.CostumeFlg4.ToArray(),
            [nameof(UserSaveDataBlue.CostumeFlg5)] = saveData.CostumeFlg5.ToArray(),
            [nameof(UserSaveDataBlue.TitleFlg)] = saveData.TitleFlg.ToArray()
        };

    private static byte[] GetUnlockField(UserSaveDataBlue saveData, string field)
        => field switch
        {
            nameof(UserSaveDataBlue.ReleaseSongFlg) => saveData.ReleaseSongFlg,
            nameof(UserSaveDataBlue.ToneFlg) => saveData.ToneFlg,
            nameof(UserSaveDataBlue.CostumeFlg1) => saveData.CostumeFlg1,
            nameof(UserSaveDataBlue.CostumeFlg2) => saveData.CostumeFlg2,
            nameof(UserSaveDataBlue.CostumeFlg3) => saveData.CostumeFlg3,
            nameof(UserSaveDataBlue.CostumeFlg4) => saveData.CostumeFlg4,
            nameof(UserSaveDataBlue.CostumeFlg5) => saveData.CostumeFlg5,
            nameof(UserSaveDataBlue.TitleFlg) => saveData.TitleFlg,
            _ => throw new InvalidOperationException($"Unsupported Blue unlock field {field}.")
        };

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        return new DefaultHttpContext { RequestServices = services };
    }
}
