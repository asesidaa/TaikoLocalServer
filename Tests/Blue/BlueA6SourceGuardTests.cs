namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueA6SourceGuardTests
{
    private static readonly string[] ForbiddenGreenShopReferences =
    [
        "GreenShop",
        "GreenShopItemStatus",
        "GreenProtocolBytes",
        "Adapters.GameProtocol.Green",
        "UserSaveDataGreen",
        "GreenShopSeasonStates",
        "GreenShopItemStates"
    ];

    [Fact]
    public void BlueA6ProductionCode_DoesNotDependOnGreenShopStateOrProtocolTypes()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Application", "Abstractions", "ITaikoDbContext.Blue.cs"),
            Path.Combine(root, "Application", "Common", "BlueShopStateExtensions.cs"),
            Path.Combine(root, "Application", "Common", "BlueShopUnlocks.cs"),
            Path.Combine(root, "Application", "Handlers", "BaidQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "GetInitialDataQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "GetItemShopInfoQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "ItemPurchaseCommand.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "UpdatePlayResultCommand.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "UserDataQuery.Blue.cs"),
            Path.Combine(root, "Domain", "Entities", "BlueShopItemState.cs"),
            Path.Combine(root, "Domain", "Entities", "BlueShopSeasonState.cs"),
            Path.Combine(root, "Domain", "Enums", "BlueShopItemStatus.cs"),
            Path.Combine(root, "Infrastructure", "GameDataCatalog", "Blue", "BlueRewardShopDataParser.cs"),
            Path.Combine(root, "Infrastructure", "Persistence", "TaikoDbContext.Blue.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers", "GetItemShopInfoController.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers", "ItemPurchaseController.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers", "RewardExecutionController.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "ItemShopMappers.cs")
        };

        AssertFilesDoNotContainForbiddenReferences(files);
    }

    [Fact]
    public void BlueA6Tests_DependOnBlueShopStateOnly()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Tests", "Blue", "BlueRewardShopDataParserTests.cs"),
            Path.Combine(root, "Tests", "Blue", "BlueItemShopLoaderTests.cs"),
            Path.Combine(root, "Tests", "Blue", "BlueItemShopProtocolTests.cs"),
            Path.Combine(root, "Tests", "Blue", "BlueItemShopPurchaseTests.cs"),
            Path.Combine(root, "Tests", "Blue", "BlueItemShopStateTests.cs"),
            Path.Combine(root, "Tests", "Blue", "BlueItemShopLockingTests.cs")
        };

        AssertFilesDoNotContainForbiddenReferences(files);
    }

    [Fact]
    public void BlueA6DefaultShopData_DoesNotReferenceGreenOrLocalBinarySources()
    {
        var root = FindRepoRoot();
        var source = File.ReadAllText(Path.Combine(
            root,
            "Host",
            "wwwroot",
            "data",
            "blue",
            "blue_item_shop_data.json"));

        foreach (var forbidden in ForbiddenGreenShopReferences)
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("rewardshopdata.bin", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SharedShopDispatchers_RouteBlueExplicitly()
    {
        var root = FindRepoRoot();
        var getItemShopInfoSource = File.ReadAllText(Path.Combine(
            root,
            "Application",
            "Handlers",
            "GetItemShopInfoQuery.cs"));
        var itemPurchaseSource = File.ReadAllText(Path.Combine(
            root,
            "Application",
            "Handlers",
            "ItemPurchaseCommand.cs"));

        Assert.Contains("GameEra.Blue => HandleBlue(request, cancellationToken)", getItemShopInfoSource, StringComparison.Ordinal);
        Assert.Contains("GameEra.Blue => HandleBlue(request, cancellationToken)", itemPurchaseSource, StringComparison.Ordinal);
    }

    private static void AssertFilesDoNotContainForbiddenReferences(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            foreach (var forbidden in ForbiddenGreenShopReferences)
            {
                Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
            }
        }
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
}
