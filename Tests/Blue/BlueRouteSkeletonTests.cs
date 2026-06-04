namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueRouteSkeletonTests
{
    private static readonly string[] ExpectedBlueGameRoutes =
    [
        "/v10r03/chassis/initialdatacheck.php",
        "/v10r03/chassis/tournamentcheck.php",
        "/v10r03/chassis/bookkeeping.php",
        "/v10r03/chassis/coinsetting.php",
        "/v10r03/chassis/gettelop.php",
        "/v10r03/chassis/getfolder.php",
        "/v10r03/chassis/taikojuku.php",
        "/v10r03/chassis/getitemshopinfo.php",
        "/v10r03/chassis/headclerk2.php",
        "/v10r03/chassis/playresult.php",
        "/v10r03/chassis/banacoinerrorlog.php",
        "/v10r03/chassis/getbanacoininfo.php",
        "/v10r03/chassis/baidcheck.php",
        "/v10r03/chassis/mydonentry.php",
        "/v10r03/chassis/userdata.php",
        "/v10r03/chassis/challengecompe.php",
        "/v10r03/chassis/balancecheck.php",
        "/v10r03/chassis/banacoinpayment.php",
        "/v10r03/chassis/crownsdata.php",
        "/v10r03/chassis/recommend.php",
        "/v10r03/chassis/selfbest.php",
        "/v10r03/chassis/heartbeat.php",
        "/v10r03/chassis/itempurchase.php",
        "/v10r03/chassis/battleuserdata.php",
        "/v10r03/chassis/rewardcardcheck.php",
        "/v10r03/chassis/rewardexecution.php"
    ];

    [Fact]
    public void BlueGameRoutes_AreOwnedByBlueAdapter()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
            .Where(route => route.Template.StartsWith("/v10r03/chassis", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedBlueGameRoutes.Order(StringComparer.Ordinal).ToArray(), routes);
    }

    [Theory]
    [InlineData("/v10r03/chassis/getreitai.php")]
    [InlineData("/v10r03/chassis/startupauth.php")]
    [InlineData("/v10r03/chassis/verupauth.php")]
    [InlineData("/v10r03/chassis/verupcomplete.php")]
    public void BlueAdapter_DoesNotOwnExcludedOrSharedRoutes(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        Assert.Empty(routes);
    }

    [Fact]
    public void BlueAdapter_OnlyOwnsImplementedDedicatedBattleRoutes()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
            .Where(route => route.Template.Contains("battle", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["/v10r03/chassis/battleuserdata.php"], routes);
    }

    [Fact]
    public void BlueControllers_DoNotCallMediatorOutsideImplementedEndpoints()
    {
        var root = FindRepoRoot();
        var controllersRoot = Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers");
        var mediatorBackedControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BaidController.cs",
            "MyDonEntryController.cs",
            "InitialDataCheckController.cs",
            "UserDataController.cs",
            "PlayResultController.cs",
            "SelfBestController.cs",
            "TaikojukuController.cs",
            "RewardCardCheckController.cs",
            "GetItemShopInfoController.cs",
            "ItemPurchaseController.cs",
            "GetFolderController.cs",
            "GetTelopController.cs",
            "BattleUserDataController.cs"
        };

        foreach (var file in Directory.EnumerateFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (mediatorBackedControllers.Contains(Path.GetFileName(file)))
            {
                continue;
            }

            var source = File.ReadAllText(file);
            Assert.DoesNotContain("Mediator.Send", source, StringComparison.Ordinal);
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
