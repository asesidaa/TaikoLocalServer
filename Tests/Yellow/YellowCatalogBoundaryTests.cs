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
        "PlayResultController",
        "SelfBestController",
        "ItemPurchaseController"
    ];

    private static readonly string[] YellowStateBackedControllers =
    [
        "CrownsDataController"
    ];

    private static readonly string[] DeferredNoStateControllers =
    [
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
    public void Phase13To15ImplementedControllers_CallRuntimeBehavior()
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
    public void YellowImplementation_DoesNotIntroduceBattleOrBanacoinAuthorityFiles()
    {
        var root = FindRepoRoot();
        var searchedRoots = new[]
        {
            Path.Combine(root, "Application"),
            Path.Combine(root, "Domain", "Entities"),
            Path.Combine(root, "Infrastructure", "Persistence")
        };
        var forbiddenTokens = new[]
        {
            "YellowBattle",
            "BanacoinWallet"
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
