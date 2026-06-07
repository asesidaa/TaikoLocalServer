using System.Text.RegularExpressions;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowCatalogBoundaryTests
{
    private static readonly string[] MediatorBackedControllers =
    [
        "InitialDataCheckController",
        "GetTelopController",
        "GetFolderController",
        "TaikojukuController",
        "GetItemShopInfoController",
        "RecommendController",
        "TournamentCheckController",
        "ChallengeCompeController",
        "BaidController",
        "MyDonEntryController",
        "UserDataController",
        "SelfBestController"
    ];

    private static readonly string[] YellowStateBackedControllers =
    [
        "CrownsDataController"
    ];

    private static readonly string[] DeferredNoStateControllers =
    [
        "PlayResultController",
        "ItemPurchaseController",
        "RewardCardCheckController",
        "RewardExecutionController",
        "BookkeepingController",
        "CoinSettingController",
        "HeadClerk2Controller",
        "HeartbeatController",
        "BalanceCheckController",
        "BanacoinPaymentController",
        "BanacoinErrorLogController",
        "GetBanacoinInfoController"
    ];

    [Fact]
    public void Phase13MetadataAndPhase14ReadbackControllers_CallRuntimeBehavior()
    {
        var source = File.ReadAllText(Path.Combine(FindRepoRoot(), "Adapters.GameProtocol.Yellow", "Controllers", "YellowScaffoldControllers.cs"));

        Assert.Equal(MediatorBackedControllers.Length, Regex.Matches(source, "Mediator\\.Send").Count);
        foreach (var controller in MediatorBackedControllers)
        {
            Assert.Contains("Mediator.Send", ExtractControllerSource(source, controller), StringComparison.Ordinal);
        }

        foreach (var controller in YellowStateBackedControllers)
        {
            Assert.Contains("SongBestDataYellow", ExtractControllerSource(source, controller), StringComparison.Ordinal);
        }

        foreach (var controller in DeferredNoStateControllers)
        {
            Assert.DoesNotContain("Mediator.Send", ExtractControllerSource(source, controller), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void YellowImplementation_DoesNotIntroduceDeferredPhase15Or16OrBattleFiles()
    {
        var root = FindRepoRoot();
        var forbiddenFiles = new[]
        {
            Path.Combine(root, "Application", "Handlers", "ItemPurchaseCommand.Yellow.cs"),
            Path.Combine(root, "Domain", "Entities", "YellowShopSeasonState.cs"),
            Path.Combine(root, "Domain", "Entities", "YellowShopItemState.cs"),
            Path.Combine(root, "Domain", "Entities", "YellowTokkunStageResult.cs")
        };

        foreach (var file in forbiddenFiles)
        {
            Assert.False(File.Exists(file), $"Deferred Phase 14-16 file exists: {file}");
        }

        var searchedRoots = new[]
        {
            Path.Combine(root, "Application"),
            Path.Combine(root, "Domain", "Entities"),
            Path.Combine(root, "Infrastructure", "Persistence")
        };
        var forbiddenTokens = new[]
        {
            "YellowBattle",
            "YellowTokkun",
            "BanacoinWallet",
            "ItemPurchaseCommand.Yellow"
        };

        foreach (var file in searchedRoots.SelectMany(path => Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)))
        {
            var source = File.ReadAllText(file);
            foreach (var token in forbiddenTokens)
            {
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
            }
        }
    }

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
}
