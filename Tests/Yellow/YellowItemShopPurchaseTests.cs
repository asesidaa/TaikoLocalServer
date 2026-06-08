using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using YellowWire = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowItemShopPurchaseTests
{
    [Fact]
    public async Task GetOrCreateActiveYellowShopSeasonState_FirstTouchSeedsYellowSaveDonMedalTotals()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.TotalGetDonmedal = 120;
        save.TotalUseDonmedal = 45;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateActiveYellowShopSeasonStateAsync(
            save,
            CreateSingleItemShopCatalog(),
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.NotNull(state);
        Assert.Equal(2u, state!.SeasonId);
        Assert.Equal(120u, state.TotalGetDonmedal);
        Assert.Equal(45u, state.TotalUseDonmedal);
    }

    [Fact]
    public async Task GetOrCreateActiveYellowShopSeasonState_DisabledShopDoesNotCreateState()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.TotalGetDonmedal = 120;
        save.TotalUseDonmedal = 45;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateActiveYellowShopSeasonStateAsync(
            save,
            YellowItemShopCatalog.Disabled,
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Null(state);
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.ToListAsync());
    }

    [Fact]
    public async Task GetOrCreateActiveYellowShopSeasonState_EmptyActiveSeasonDoesNotCreateState()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();

        var catalog = new YellowItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, YellowItemShopSeason>
            {
                [2] = new() { SeasonId = 2, Items = [] }
            }
        };

        var state = await fixture.Context.GetOrCreateActiveYellowShopSeasonStateAsync(
            save,
            catalog,
            CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Null(state);
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.ToListAsync());
    }

    [Fact]
    public async Task GetUnlockedYellowShopItemsAsync_ReturnsOnlyYellowUnlockedItemsForSeason()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.YellowShopItemStates.AddRange(
            Unlocked(1, 2, 3, 12),
            Unlocked(1, 3, 3, 9),
            new YellowShopItemState
            {
                Baid = 1,
                SeasonId = 2,
                ItemType = 3,
                ItemId = 10,
                ItemNo = 3,
                ItemPrice = 1500,
                Status = (YellowShopItemStatus)0,
                PurchasedAt = DateTime.UtcNow
            });
        fixture.Context.BlueShopItemStates.Add(new BlueShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 3,
            ItemId = 99,
            ItemNo = 99,
            ItemPrice = 1500,
            Status = BlueShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow
        });
        fixture.Context.GreenShopItemStates.Add(new GreenShopItemState
        {
            Baid = 1,
            SeasonId = 2,
            ItemType = 3,
            ItemId = 100,
            ItemNo = 100,
            ItemPrice = 1500,
            Status = GreenShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var items = await fixture.Context.GetUnlockedYellowShopItemsAsync(1, 2, CancellationToken.None);

        Assert.Contains((3u, 12u), items);
        Assert.DoesNotContain((3u, 9u), items);
        Assert.DoesNotContain((3u, 10u), items);
        Assert.DoesNotContain((3u, 99u), items);
        Assert.DoesNotContain((3u, 100u), items);
    }

    [Fact]
    public async Task ItemPurchase_PreflightReturnsYellowActiveSeasonBalanceWithoutSpending()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 700, totalUseDonmedal: 200);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, 0, null, null, null), CancellationToken.None);

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(700u, response.TotalGetDonmedal);
        Assert.Equal(200u, response.TotalUseDonmedal);
        Assert.Equal(200u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.YellowShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_ValidYellowPurchaseSpendsDonmedalsAndPersistsUnlockedItem()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, 1, 3, 12, 1300), CancellationToken.None);

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        var item = await fixture.Context.YellowShopItemStates.FindAsync(1u, 2u, 3u, 12u);
        var save = await fixture.Context.UserSaveDataYellow.FindAsync(1u);
        Assert.Equal(1u, response.Result);
        Assert.Equal(2000u, response.TotalGetDonmedal);
        Assert.Equal(1300u, response.TotalUseDonmedal);
        Assert.Equal(1300u, season!.TotalUseDonmedal);
        Assert.Equal(YellowShopItemStatus.Unlocked, item!.Status);
        Assert.Equal(1u, item.ItemNo);
        Assert.Equal(1300u, item.ItemPrice);
        Assert.NotNull(item.UnlockedAt);
        Assert.Equal(0u, save!.TotalGetDonmedal);
        Assert.Equal(0u, save.TotalUseDonmedal);
        Assert.True(HasBit(save.CostumeFlg1, 12));
    }

    [Theory]
    [InlineData(2, 3, 12, 1300)]
    [InlineData(1, 4, 12, 1300)]
    [InlineData(1, 3, 13, 1300)]
    [InlineData(1, 3, 12, 1500)]
    public async Task ItemPurchase_RejectsForgedYellowCatalogTupleWithoutMutation(
        uint itemNo,
        uint itemType,
        uint itemId,
        uint itemPrice)
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, itemNo, itemType, itemId, itemPrice), CancellationToken.None);

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        var save = await fixture.Context.UserSaveDataYellow.FindAsync(1u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.YellowShopItemStates.AnyAsync());
        Assert.False(HasBit(save!.CostumeFlg1, 12));
    }

    [Fact]
    public async Task ItemPurchase_RejectsZeroPriceYellowRowsWithoutMutation()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 0 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, 1, 3, 12, 0), CancellationToken.None);

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.YellowShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_RejectsInsufficientYellowDonmedalsWithoutMutation()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 1200);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, 1, 3, 12, 1300), CancellationToken.None);

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.YellowShopItemStates.AnyAsync());
    }

    [Fact]
    public async Task ItemPurchase_RejectsUnsupportedYellowItemTypeWithoutMutation()
    {
        var unsupportedItemType = (Ac15ShopItemType)99;
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = unsupportedItemType, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, 1, 99, 12, 1300), CancellationToken.None);

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        var save = await fixture.Context.UserSaveDataYellow.FindAsync(1u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(0u, season!.TotalUseDonmedal);
        Assert.False(await fixture.Context.YellowShopItemStates.AnyAsync());
        Assert.False(HasBit(save!.CostumeFlg1, 12));
    }

    [Fact]
    public async Task ItemPurchase_RejectsDuplicateYellowUnlockedWithoutDoubleSpend()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }));
        await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000, totalUseDonmedal: 1300);
        fixture.Context.YellowShopItemStates.Add(Unlocked(1, 2, 3, 12));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, 1, 3, 12, 1300), CancellationToken.None);

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        Assert.Equal(0u, response.Result);
        Assert.Equal(1300u, season!.TotalUseDonmedal);
        Assert.Single(await fixture.Context.YellowShopItemStates.ToListAsync());
    }

    [Theory]
    [InlineData(Ac15ShopItemType.Song, 101, nameof(UserSaveDataYellow.ReleaseSongFlg))]
    [InlineData(Ac15ShopItemType.Tone, 4, nameof(UserSaveDataYellow.ToneFlg))]
    [InlineData(Ac15ShopItemType.Kigurumi, 12, nameof(UserSaveDataYellow.CostumeFlg1))]
    [InlineData(Ac15ShopItemType.Body, 13, nameof(UserSaveDataYellow.CostumeFlg3))]
    [InlineData(Ac15ShopItemType.Head, 14, nameof(UserSaveDataYellow.CostumeFlg2))]
    [InlineData(Ac15ShopItemType.Face, 15, nameof(UserSaveDataYellow.CostumeFlg4))]
    [InlineData(Ac15ShopItemType.Puchi, 16, nameof(UserSaveDataYellow.CostumeFlg5))]
    public async Task ItemPurchase_UnlocksSupportedItemTypesInExactYellowSaveField(
        Ac15ShopItemType itemType,
        uint itemId,
        string expectedField)
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = itemType, ItemId = itemId, Price = 100 }));
        var save = await AddUserWithSeasonAsync(fixture, totalGetDonmedal: 2000);
        var before = SnapshotUnlockFields(save);
        var handler = CreateHandler(fixture);

        var response = await handler.Handle(new ItemPurchaseCommand(1, GameEra.Yellow, 1, itemType.ToProtocolValue(), itemId, 100), CancellationToken.None);

        var reloaded = await fixture.Context.UserSaveDataYellow.FindAsync(1u);
        Assert.Equal(1u, response.Result);
        foreach (var (field, bytes) in before)
        {
            var current = GetUnlockField(reloaded!, field);
            if (field == expectedField)
            {
                Assert.True(HasBit(current, itemId));
                Assert.NotEqual(bytes, current);
            }
            else
            {
                Assert.Equal(bytes, current);
            }
        }
    }

    [Fact]
    public void ItemPurchaseCommandMap_PreservesOmittedOptionalDetailsAndYellowEra()
    {
        var request = new YellowWire.ItempurchaseRequest
        {
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            Baid = 1,
            ItemNo = 0
        };

        var command = ItemShopMappers.Map(request);

        Assert.Equal(1u, command.Baid);
        Assert.Equal(GameEra.Yellow, command.Era);
        Assert.Equal(0u, command.ItemNo);
        Assert.Null(command.ItemType);
        Assert.Null(command.ItemId);
        Assert.Null(command.ItemPrice);
    }

    [Fact]
    public void ItemPurchaseCommandMap_PreservesExplicitZeroOptionalDetails()
    {
        var request = new YellowWire.ItempurchaseRequest
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
    public void ItemPurchaseResponseMap_MapsYellowTotals()
    {
        var response = ItemShopMappers.Map(new CommonItemPurchaseResponse
        {
            Result = 1,
            TotalGetDonmedal = 700,
            TotalUseDonmedal = 200
        });

        Assert.Equal(1u, response.Result);
        Assert.Equal(700u, response.TotalGetDonmedal);
        Assert.Equal(200u, response.TotalUseDonmedal);
    }

    [Fact]
    public void YellowItemPurchaseController_UsesMediatorCommandAndResponseMapper()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "Adapters.GameProtocol.Yellow",
            "Controllers",
            "YellowScaffoldControllers.cs"));
        var controller = ExtractControllerSource(source, "ItemPurchaseController");

        Assert.Contains("Task<IActionResult> ItemPurchase", controller, StringComparison.Ordinal);
        Assert.Contains("Mediator.Send(ItemShopMappers.Map(request)", controller, StringComparison.Ordinal);
        Assert.Contains("return Ok(ItemShopMappers.Map(common));", controller, StringComparison.Ordinal);
        Assert.DoesNotContain("new ItempurchaseResponse { Result = 1 }", controller, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("rewardcardcheck")]
    [InlineData("rewardexecution")]
    public async Task YellowRewardCompatibilityRoutes_ReturnSuccessWithoutMutatingShopOrSaveState(string route)
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateShopCatalog(
            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Kigurumi, ItemId = 12, Price = 1300 }));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, [4], Ac15EraProfiles.Yellow.Limits.ToneFlagBytes);
        save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [12], Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes);
        fixture.Context.UserSaveDataYellow.Add(save);
        fixture.Context.YellowShopSeasonStates.Add(new YellowShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = 700,
            TotalUseDonmedal = 200,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        fixture.Context.YellowShopItemStates.Add(Unlocked(1, 2, 3, 12));
        await fixture.Context.SaveChangesAsync();
        var unlockFieldsBefore = SnapshotUnlockFields(save);

        var response = route == "rewardcardcheck"
            ? InvokeRewardCardCheck()
            : InvokeRewardExecution();

        var season = await fixture.Context.YellowShopSeasonStates.FindAsync(1u, 2u);
        var reloaded = await fixture.Context.UserSaveDataYellow.FindAsync(1u);
        Assert.Equal(1u, response);
        Assert.Equal(700u, season!.TotalGetDonmedal);
        Assert.Equal(200u, season.TotalUseDonmedal);
        foreach (var (field, bytes) in unlockFieldsBefore)
        {
            Assert.Equal(bytes, GetUnlockField(reloaded!, field));
        }

        Assert.Single(await fixture.Context.YellowShopItemStates.ToListAsync());

        uint InvokeRewardCardCheck()
        {
            var controller = new RewardCardCheckController
            {
                ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
            };

            var result = controller.RewardCardCheck(new YellowWire.RewardcardcheckRequest
            {
                DeviceType = 1,
                AccessCode = "12345678901234567890",
                ChipId = "chip",
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123",
                CountryId = "JPN"
            });
            var ok = Assert.IsType<OkObjectResult>(result);
            return Assert.IsType<YellowWire.RewardcardcheckResponse>(ok.Value).Result;
        }

        uint InvokeRewardExecution()
        {
            var controller = new RewardExecutionController
            {
                ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
            };

            var result = controller.RewardExecution(new YellowWire.RewardexecutionRequest
            {
                Baid = 1,
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123",
                ReleaseSongNoes = [101],
                GetToneNoes = [5],
                GetCostumeNo1s = [13]
            });
            var ok = Assert.IsType<OkObjectResult>(result);
            return Assert.IsType<YellowWire.RewardexecutionResponse>(ok.Value).Result;
        }
    }

    private static YellowShopItemState Unlocked(uint baid, uint seasonId, uint itemType, uint itemId) => new()
    {
        Baid = baid,
        SeasonId = seasonId,
        ItemType = itemType,
        ItemId = itemId,
        ItemNo = itemId,
        ItemPrice = 1500,
        Status = YellowShopItemStatus.Unlocked,
        PurchasedAt = DateTime.UtcNow,
        UnlockedAt = DateTime.UtcNow
    };

    private static ItemPurchaseCommandHandler CreateHandler(YellowHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<ItemPurchaseCommandHandler>.Instance);

    private static async Task<UserSaveDataYellow> AddUserWithSeasonAsync(
        YellowHandlerFixture fixture,
        uint totalGetDonmedal,
        uint totalUseDonmedal = 0)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        fixture.Context.UserSaveDataYellow.Add(save);
        fixture.Context.YellowShopSeasonStates.Add(new YellowShopSeasonState
        {
            Baid = 1,
            SeasonId = 2,
            TotalGetDonmedal = totalGetDonmedal,
            TotalUseDonmedal = totalUseDonmedal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        return save;
    }

    private static YellowHandlerFixture.TestYellowCatalog CreateShopCatalog(params YellowItemShopEntry[] items)
        => new(itemShopCatalog: new YellowItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, YellowItemShopSeason>
            {
                [2] = new()
                {
                    SeasonId = 2,
                    VerupNo = 20110301,
                    StartDatetime = "20110301070000",
                    EndDatetime = "20110630020000",
                    Items = items
                }
            }
        });

    private static YellowItemShopCatalog CreateSingleItemShopCatalog()
    {
        var season = new YellowItemShopSeason
        {
            SeasonId = 2,
            Items = [new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 101, Price = 1300 }]
        };

        return new YellowItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, YellowItemShopSeason> { [2] = season }
        };
    }

    private static Dictionary<string, byte[]> SnapshotUnlockFields(UserSaveDataYellow saveData)
        => new()
        {
            [nameof(UserSaveDataYellow.ReleaseSongFlg)] = saveData.ReleaseSongFlg.ToArray(),
            [nameof(UserSaveDataYellow.ToneFlg)] = saveData.ToneFlg.ToArray(),
            [nameof(UserSaveDataYellow.CostumeFlg1)] = saveData.CostumeFlg1.ToArray(),
            [nameof(UserSaveDataYellow.CostumeFlg2)] = saveData.CostumeFlg2.ToArray(),
            [nameof(UserSaveDataYellow.CostumeFlg3)] = saveData.CostumeFlg3.ToArray(),
            [nameof(UserSaveDataYellow.CostumeFlg4)] = saveData.CostumeFlg4.ToArray(),
            [nameof(UserSaveDataYellow.CostumeFlg5)] = saveData.CostumeFlg5.ToArray()
        };

    private static byte[] GetUnlockField(UserSaveDataYellow saveData, string field)
        => field switch
        {
            nameof(UserSaveDataYellow.ReleaseSongFlg) => saveData.ReleaseSongFlg,
            nameof(UserSaveDataYellow.ToneFlg) => saveData.ToneFlg,
            nameof(UserSaveDataYellow.CostumeFlg1) => saveData.CostumeFlg1,
            nameof(UserSaveDataYellow.CostumeFlg2) => saveData.CostumeFlg2,
            nameof(UserSaveDataYellow.CostumeFlg3) => saveData.CostumeFlg3,
            nameof(UserSaveDataYellow.CostumeFlg4) => saveData.CostumeFlg4,
            nameof(UserSaveDataYellow.CostumeFlg5) => saveData.CostumeFlg5,
            _ => throw new InvalidOperationException($"Unsupported Yellow unlock field {field}.")
        };

    private static bool HasBit(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private static string ExtractControllerSource(string source, string controllerName)
    {
        var start = source.IndexOf($"class {controllerName}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Controller {controllerName} not found.");
        var next = source.IndexOf("\n[ApiController]", start, StringComparison.Ordinal);
        return next >= 0 ? source[start..next] : source[start..];
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        return new DefaultHttpContext { RequestServices = services };
    }
}
